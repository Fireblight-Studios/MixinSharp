namespace MixinSharp;

/// <summary>
/// Denotes that the class should include a Mixin.
/// </summary>
/// <remarks>
/// Only valid on partial classes.
/// </remarks>
[AttributeUsage(AttributeTargets.Class,  AllowMultiple = true)]
public class UseMixinAttribute:Attribute
{
    /// <summary>
    /// Type of the Mixin to be injected in at compile time.
    /// </summary>
    public Type MixinType { get; }

    /// <summary>
    /// Specifies that the class should include a mixin.
    /// </summary>
    /// <remarks>
    /// This attribute is valid only for partial classes.
    /// </remarks>
    /// <param name="mixinType">Type of the Mixin to include.</param>
    public UseMixinAttribute(Type mixinType)
    {
        MixinType = mixinType;
    }
}