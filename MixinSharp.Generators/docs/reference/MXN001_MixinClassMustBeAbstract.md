# MXN001: Mixin class must be abstract

## Cause
A class is annotated with `DefineMixinAttribute` (e.g., `[DefineMixin]`) but is not declared `abstract`.

## Rule description
Mixin types define reusable members that are copied into consumer classes via `[UseMixin]`. To prevent instantiation and to make intent explicit, mixin classes must be declared `abstract`. When a non-abstract class is marked with `[DefineMixin]`, the generator emits diagnostic `MXN001` and stops generating the mixin interface for that type.

## How to fix violations
- Add the `abstract` modifier to the mixin class declaration.
- If the type is not intended to be a mixin, remove the `[DefineMixin]` attribute.

## When to suppress warnings
Suppress only if you intentionally treat the type as a normal concrete class and you do not want it to participate in mixin generation. In that case, remove `[DefineMixin]` instead of suppressing the diagnostic.

## Example of a violation

### Description
`TestMixinA` is marked with `[DefineMixin]` but is not `abstract`.

### Code
```csharp
using MixinSharp;

namespace Samples;

[DefineMixin]
public class TestMixinA // ❌ not abstract
{
    public void Hello() { }
}
```

## Example of how to fix

### Description
Declare the mixin as `abstract`.

### Code
```csharp
using MixinSharp;

namespace Samples;

[DefineMixin]
public abstract class TestMixinA // ✅ abstract
{
    public void Hello() { }
}
```

## Related rules
[MXN002: Mixin class must not inherit a class](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN002_MixinClassMustNotInheritAClass.md)
[MXN004: UseMixin type must be a mixin](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN004_UseMixinTypeMustBeAMixin.md)