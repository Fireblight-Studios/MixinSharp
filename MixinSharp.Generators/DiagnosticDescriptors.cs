using Microsoft.CodeAnalysis;

namespace MixinSharp.Generators;

/// <summary>
/// Centralized diagnostic descriptors used by the MixinSharp source generators.
/// </summary>
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor NonAbstractMixinError = new(
        id: "MXN001",
        title: "Mixin class must be abstract",
        messageFormat: "Class '{0}' is marked with [DefineMixin] but is not abstract",
        category: "MixinSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MixinInheritsClassError = new(
        id: "MXN002",
        title: "Mixin class must not inherit a class",
        messageFormat: "Mixin '{0}' must not inherit from another class (only 'object' is allowed)",
        category: "MixinSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UseMixinNotPartialError = new(
        id: "MXN003",
        title: "Class using mixin must be partial",
        messageFormat: "Class '{0}' uses [UseMixin] but is not declared partial",
        category: "MixinSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UseMixinNotAMixinError = new(
        id: "MXN004",
        title: "UseMixin type must be a mixin",
        messageFormat: "Type '{0}' supplied to [UseMixin] is not a valid mixin (missing [DefineMixin])",
        category: "MixinSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MemberConflictError = new(
        id: "MXN005",
        title: "Member conflict when applying mixin",
        messageFormat: "Class '{0}' already contains a member named '{1}' required by mixin '{2}'",
        category: "MixinSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
