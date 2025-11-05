namespace MixinSharp;

/// <summary>
/// Denotes that the method should act as a pseudo constructor for the mixin when injected into another class.
/// Any classes that use this Mixin will be required to call this method in all of their constructors.
/// </summary>
/// <remarks>
/// A Mixin can only have one method defined as a MixinConstructor.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MixinConstructor : Attribute
{
    
}