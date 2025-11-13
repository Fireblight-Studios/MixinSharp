using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FireblightStudios.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MixinSharp.Generators;

[Generator(LanguageNames.CSharp)]
public class MixinGenerator : IIncrementalGenerator
{
    private const string DefineMixinAttr = "MixinSharp.DefineMixinAttribute";
    private const string UseMixinAttr = "MixinSharp.UseMixinAttribute";


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Debug: marker to verify generator loading
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource("MixinGenerator_Alive.g.cs", "// MixinGenerator loaded\n");
        });

        // Collect mixin definitions
        var mixinCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: DefineMixinAttr,
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                var classDecl = (ClassDeclarationSyntax)ctx.TargetNode;
                var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                return new MixinDef(symbol, classDecl);
            });

        // Collect consumers with [UseMixin]
        var consumerCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: UseMixinAttr,
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                var classDecl = (ClassDeclarationSyntax)ctx.TargetNode;
                var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                var attributeData = ctx.Attributes.FirstOrDefault();
                return new UseMixinDef(symbol, classDecl, attributeData);
            });

        // Generate mixin interfaces
        context.RegisterSourceOutput(mixinCandidates.Collect(), (spc, list) =>
        {
            var handled = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var item in list)
            {
                if (item.Symbol is null) continue;
                if (!handled.Add(item.Symbol)) continue;
                ValidateAndGenerateMixinInterface(spc, item.Symbol, item.Declaration);
            }
        });

        // Generate consumer augmentations
        var mixinMapProvider = mixinCandidates.Collect();
        var consumerListProvider = consumerCandidates.Collect();
        context.RegisterSourceOutput(consumerListProvider.Combine(mixinMapProvider), (spc, pair) =>
        {
            var consumers = pair.Left;
            var allMixins = pair.Right;
            // Group by consumer symbol to aggregate multiple [UseMixin] attributes
            foreach (var group in consumers
                         .Where(c => c.Symbol is not null)
                         .GroupBy(c => c.Symbol, SymbolEqualityComparer.Default))
            {
                var consumerSymbol = (INamedTypeSymbol)group.Key!;
                var declaration = group.Select(g => g.Declaration).FirstOrDefault(d => d is not null);
                var attrs = group.Select(g => g.AttributeData).Where(a => a is not null).Cast<AttributeData>();
                GenerateConsumerAugmentation(spc, consumerSymbol, declaration, attrs, allMixins);
            }
        });
    }

    private static void ValidateAndGenerateMixinInterface(SourceProductionContext spc, INamedTypeSymbol mixinSymbol, ClassDeclarationSyntax? mixinDecl)
    {
        // Validate abstract
        if (!mixinSymbol.IsAbstract)
        {
            spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NonAbstractMixinError, mixinSymbol.Locations.FirstOrDefault(), mixinSymbol.Name));
            return;
        }
        // Validate no base class other than object
        var baseType = mixinSymbol.BaseType;
        if (baseType != null && baseType.SpecialType != SpecialType.System_Object)
        {
            spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MixinInheritsClassError, mixinSymbol.Locations.FirstOrDefault(), mixinSymbol.Name));
            return;
        }

        // Build interface code from public methods/properties
        var ns = mixinSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        var ifaceName = "I_Mx" + mixinSymbol.Name;
        var sb = new CodeBuilder();
        if (!string.IsNullOrEmpty(ns))
        {
            sb.Append("namespace ").Append(ns).AppendLine(";");
            sb.AppendLine();
        }
        sb.Append("public interface ").Append(ifaceName);

        // Include mixin interfaces as inherited interfaces too
        var mixinIfaces = mixinSymbol.Interfaces;
        if (mixinIfaces.Length > 0)
        {
            sb.Append(" : ").Append(string.Join(", ", mixinIfaces.Select(i => i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty))));
        }
        sb.AppendLine().IncreaseIndentation().AppendLine("{");

        // Build a set of members that the mixin already implements from its interfaces
        var ifaceMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var iface in mixinSymbol.AllInterfaces)
        {
            foreach (var im in iface.GetMembers())
            {
                ifaceMembers.Add(im);
            }
        }

        var implementedInterfaceMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var im in ifaceMembers)
        {
            var impl = mixinSymbol.FindImplementationForInterfaceMember(im);
            if (impl != null && SymbolEqualityComparer.Default.Equals(impl.ContainingType, mixinSymbol))
            {
                implementedInterfaceMembers.Add(impl);
            }
        }

        // Emit only public instance members declared in the mixin that do NOT already exist on inherited interfaces
        foreach (var sym in mixinSymbol.GetMembers())
        {
            if (sym is IMethodSymbol ms)
            {
                if (ms.MethodKind != MethodKind.Ordinary) continue;
                if (ms.DeclaredAccessibility != Accessibility.Public) continue;
                if (ms.IsStatic) continue;
                if (implementedInterfaceMembers.Contains(ms)) continue; // Do not re-emit members that satisfy inherited interfaces

                // Use declaring syntax to preserve docs and attributes where appropriate
                var decl = ms.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                if (decl == null) continue;

                AppendDoc(sb, decl.GetLeadingTrivia());
                var signature = decl.WithBody(null)
                                    .WithExpressionBody(null)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                signature = signature.WithModifiers(FilterInterfaceAllowedModifiers(decl.Modifiers));
                sb.AppendLine(signature.NormalizeWhitespace().ToFullString());
            }
            else if (sym is IPropertySymbol ps)
            {
                if (ps.DeclaredAccessibility != Accessibility.Public) continue;
                if (ps.IsStatic) continue;
                if (implementedInterfaceMembers.Contains(ps)) continue; // Skip if already present via inherited interfaces

                var decl = ps.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;
                if (decl == null) continue;

                AppendDoc(sb, decl.GetLeadingTrivia());
                var accessors = decl.AccessorList;
                if (accessors != null)
                {
                    var newAccessors = SyntaxFactory.AccessorList(SyntaxFactory.List(accessors.Accessors.Select(a =>
                        SyntaxFactory.AccessorDeclaration(a.Kind()).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))));
                    var newProp = decl.WithAccessorList(newAccessors)
                                      .WithExpressionBody(null)
                                      .WithInitializer(null)
                                      .WithSemicolonToken(default);
                    newProp = newProp.WithModifiers(FilterInterfaceAllowedModifiers(decl.Modifiers));
                    sb.AppendLine(newProp.NormalizeWhitespace().ToFullString());
                }
            }
            else if (sym is IEventSymbol es)
            {
                if (es.DeclaredAccessibility != Accessibility.Public) continue;
                if (es.IsStatic) continue;
                if (implementedInterfaceMembers.Contains(es)) continue;

                var decl = es.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (decl is EventDeclarationSyntax ed)
                {
                    AppendDoc(sb, ed.GetLeadingTrivia());
                    var newEd = ed.WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(ed.AccessorList?.Accessors.Select(a =>
                                        SyntaxFactory.AccessorDeclaration(a.Kind()).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))) ?? Enumerable.Empty<AccessorDeclarationSyntax>())));
                    newEd = newEd.WithModifiers(FilterInterfaceAllowedModifiers(ed.Modifiers));
                    sb.AppendLine(newEd.NormalizeWhitespace().ToFullString());
                }
                else if (decl is EventFieldDeclarationSyntax efd)
                {
                    AppendDoc(sb, efd.GetLeadingTrivia());
                    var newEfd = efd.WithModifiers(FilterInterfaceAllowedModifiers(efd.Modifiers));
                    sb.AppendLine(newEfd.NormalizeWhitespace().ToFullString());
                }
            }
        }

        sb.LeaveScope();

        spc.AddSource($"{ifaceName}.g.cs", sb.ToString());
    }

    private static void GenerateConsumerAugmentation(
        SourceProductionContext spc,
        INamedTypeSymbol consumerSymbol,
        ClassDeclarationSyntax? consumerDecl,
        IEnumerable<AttributeData>? useMixinAttributes,
        ImmutableArray<MixinDef> allMixins)
    {
        if (consumerSymbol is null) return;

        // Validate partial
        if (!IsPartial(consumerDecl))
        {
            spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseMixinNotPartialError, consumerSymbol.Locations.FirstOrDefault(), consumerSymbol.Name));
            return;
        }

        var consumerAllInterfaces = new HashSet<string>(consumerSymbol.AllInterfaces.Select(i => i.ToDisplayString()), StringComparer.Ordinal);
        var targetInterfaces = new HashSet<string>(StringComparer.Ordinal);
        var copiedMembers = new CodeBuilder();

        // Always derive UseMixin types from the consumer symbol to capture all attributes
        var mixinTypes = consumerSymbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == UseMixinAttr)
            .Select(a => a.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol)
            .Where(t => t is not null)
            .Cast<INamedTypeSymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default)
            .ToList();

        foreach (var symbol in mixinTypes)
        {
            var mixinTypeArg = symbol as INamedTypeSymbol;
            if (mixinTypeArg == null)
            {
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseMixinNotAMixinError, consumerSymbol.Locations.FirstOrDefault(), "<unknown>"));
                continue;
            }

            // Ensure it's a mixin (has DefineMixin)
            var isMixin = mixinTypeArg.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == DefineMixinAttr);
            if (!isMixin)
            {
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseMixinNotAMixinError, consumerSymbol.Locations.FirstOrDefault(), mixinTypeArg.ToDisplayString()));
                continue;
            }

            // Find mixin declaration
            var mixinDef = allMixins.FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(m.Symbol, mixinTypeArg));
            var mixinDecl = mixinDef.Declaration ?? mixinTypeArg.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ClassDeclarationSyntax;

            // Add inherited interfaces from mixin
            foreach (var iface in mixinTypeArg.Interfaces)
            {
                var name = iface.ToDisplayString();
                if (!consumerAllInterfaces.Contains(name))
                    targetInterfaces.Add(name);
            }
            // Add generated mixin interface
            var nsStr = mixinTypeArg.ContainingNamespace?.ToDisplayString();
            var generatedIfaceName = !string.IsNullOrEmpty(nsStr)
                ? $"global::{nsStr}.I_Mx{mixinTypeArg.Name}"
                : $"I_Mx{mixinTypeArg.Name}";
            var normalizedGenIface = generatedIfaceName.Replace("global::", string.Empty);
            if (!consumerAllInterfaces.Contains(normalizedGenIface))
            {
                targetInterfaces.Add(generatedIfaceName);
            }

            // Copy members from mixin (non-abstract, non-ctor, non-static)
            if (mixinDecl != null)
            {
                foreach (var member in mixinDecl.Members)
                {
                    if (member is ConstructorDeclarationSyntax)
                        continue;

                    if (member.Modifiers.Any(SyntaxKind.StaticKeyword))
                        continue; // do not copy statics

                    // Skip abstract members (they must be implemented by the consuming class)
                    if (member.Modifiers.Any(SyntaxKind.AbstractKeyword))
                    {
                        continue;
                    }

                    // Conflict check by name (ignore abstract members)
                    var name = GetMemberName(member);
                    if (name != null && HasMemberNamed(consumerSymbol, name))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MemberConflictError, consumerSymbol.Locations.FirstOrDefault(), consumerSymbol.Name, name, mixinTypeArg.Name));
                        continue;
                    }

                    copiedMembers.AppendLine()
                        .AppendLine(member.NormalizeWhitespace().ToFullString());
                }
            }
        }

        // Generate class partial
        var ns = consumerSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        var className = consumerSymbol.Name;
        var access = AccessibilityToString(consumerSymbol.DeclaredAccessibility);
        var baseList = targetInterfaces.Count > 0 ? " : " + string.Join(", ", targetInterfaces) : string.Empty;

        var source = new CodeBuilder();
        if (!string.IsNullOrEmpty(ns))
        {
            source.Append("namespace ").Append(ns).AppendLine(";");
            source.AppendLine();
        }
        // Minimum required usings for copied member bodies
        source.AppendLine("using System;");
        source.EnterScope($"{access} partial class {className}{baseList}")
              .Append(copiedMembers.ToString())
              .LeaveScope();

        spc.AddSource($"{className}.UseMixins.g.cs", source.ToString());
    }

    private static bool IsPartial(ClassDeclarationSyntax? decl)
    {
        if (decl == null) return false;
        return decl.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private static string AccessibilityToString(Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Public: return "public";
            case Accessibility.Internal: return "internal";
            case Accessibility.Protected: return "protected";
            case Accessibility.Private: return "private";
            case Accessibility.ProtectedOrInternal: return "protected internal";
            case Accessibility.ProtectedAndInternal: return "private protected";
            default: return string.Empty;
        }
    }

    private static void AppendDoc(CodeBuilder sb, SyntaxTriviaList trivia)
    {
        var docs = trivia.Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)).ToList();
        if (docs.Count == 0)
        {
            // Try extract structured trivia
            foreach (var t in trivia)
            {
                if (t.HasStructure && t.GetStructure() is DocumentationCommentTriviaSyntax doc)
                {
                    var text = doc.ToFullString();
                    foreach (var line in text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
                    {
                        sb.Append("/// ").AppendLine(line);
                    }
                    return;
                }
            }
            return;
        }
        foreach (var t in docs)
        {
            var text = t.ToFullString();
            foreach (var line in text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
            {
                if (line.TrimStart().StartsWith("///"))
                    sb.AppendLine(line);
                else
                    sb.Append("/// ").AppendLine(line);
            }
        }
    }

    private static SyntaxTokenList FilterInterfaceAllowedModifiers(SyntaxTokenList modifiers)
    {
        var filtered = modifiers.Where(m =>
            !m.IsKind(SyntaxKind.PublicKeyword) &&
            !m.IsKind(SyntaxKind.PrivateKeyword) &&
            !m.IsKind(SyntaxKind.ProtectedKeyword) &&
            !m.IsKind(SyntaxKind.InternalKeyword) &&
            !m.IsKind(SyntaxKind.AbstractKeyword) &&
            !m.IsKind(SyntaxKind.VirtualKeyword) &&
            !m.IsKind(SyntaxKind.OverrideKeyword) &&
            !m.IsKind(SyntaxKind.SealedKeyword) &&
            !m.IsKind(SyntaxKind.PartialKeyword) &&
            !m.IsKind(SyntaxKind.ReadOnlyKeyword));
        return SyntaxFactory.TokenList(filtered);
    }

    private static bool HasPublicModifier(SyntaxTokenList modifiers) => modifiers.Any(SyntaxKind.PublicKeyword);

    private static bool HasMemberNamed(INamedTypeSymbol type, string name)
    {
        return type.GetMembers().Any(m => string.Equals(m.Name, name, StringComparison.Ordinal));
    }

    private static string? GetMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            MethodDeclarationSyntax m => m.Identifier.Text,
            PropertyDeclarationSyntax p => p.Identifier.Text,
            FieldDeclarationSyntax f => f.Declaration.Variables.FirstOrDefault()?.Identifier.Text,
            EventDeclarationSyntax e => e.Identifier.Text,
            EventFieldDeclarationSyntax ef => ef.Declaration.Variables.FirstOrDefault()?.Identifier.Text,
            _ => null
        };
    }

    private readonly struct MixinDef
    {
        public INamedTypeSymbol Symbol { get; }
        public ClassDeclarationSyntax? Declaration { get; }
        public MixinDef(INamedTypeSymbol symbol, ClassDeclarationSyntax? declaration)
        {
            Symbol = symbol;
            Declaration = declaration;
        }
    }

    private readonly struct UseMixinDef
    {
        public INamedTypeSymbol Symbol { get; }
        public ClassDeclarationSyntax? Declaration { get; }
        public AttributeData? AttributeData { get; }
        public UseMixinDef(INamedTypeSymbol symbol, ClassDeclarationSyntax? declaration, AttributeData? attributeData)
        {
            Symbol = symbol;
            Declaration = declaration;
            AttributeData = attributeData;
        }
    }
}