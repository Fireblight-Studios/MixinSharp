namespace MixinSharp;

/// <summary>
/// Denotes that a class should be considered a Mixin.
/// </summary>
/// <remarks>
/// Mixin class must be abstract.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefineMixinAttribute:Attribute
{
    
}