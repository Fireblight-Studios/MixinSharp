# MXN004: UseMixin type must be a mixin

## Cause
A `[UseMixin]` attribute is applied with a type argument that is not a valid mixin (i.e., the type is missing `[DefineMixin]`).

## Rule description
Consumer classes opt into mixin augmentation by annotating themselves with `[UseMixin(typeof(TMixin))]`. The supplied `TMixin` must be a mixin type, identified by the presence of `[DefineMixin]`. If the type does not carry `[DefineMixin]`, the generator cannot determine mixin members to copy and reports `MXN004`.

## How to fix violations
- Ensure the referenced type is annotated with `[DefineMixin]` and satisfies mixin constraints (abstract, no base class).
- If you referenced the wrong type, update the `[UseMixin]` argument to the intended mixin type.
- If you meant to share API without copying members, prefer implementing an interface directly on the consumer.

## When to suppress warnings
Do not suppress. This diagnostic indicates a configuration error. Either remove `[UseMixin]` or point it at a valid mixin.

## Example of a violation

### Description
`Helper` is not a mixin, yet it is referenced by `[UseMixin]`.

### Code
```csharp
using MixinSharp;

namespace Samples;

public class Helper // not a mixin (no [DefineMixin])
{
    public void DoWork() { }
}

[UseMixin(typeof(Helper))] // ❌ not a mixin
public partial class TestConsumer
{
}
```

## Example of how to fix

### Description
Annotate the referenced type with `[DefineMixin]` and make it an abstract mixin, or remove/replace the attribute.

### Code
```csharp
using MixinSharp;

namespace Samples;

[DefineMixin]
public abstract class HelperMixin // ✅ valid mixin
{
    public void DoWork() { }
}

[UseMixin(typeof(HelperMixin))]
public partial class TestConsumer
{
}
```

## Related rules
[MXN001: Mixin class must be abstract](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN001_MixinClassMustBeAbstract.md)
[MXN002: Mixin class must not inherit a class](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN002_MixinClassMustNotInheritAClass.md)
[MXN003: Class using mixin must be partial](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN003_ClassUsingMixinMustBePartial.md)