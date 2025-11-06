# MXN002: Mixin class must not inherit a class

## Cause
A class marked with `[DefineMixin]` inherits from a base class other than `object`.

## Rule description
Mixin classes are intended to be member providers that can be copied into multiple consumers. Allowing mixins to inherit from concrete or abstract base classes would introduce complex and conflicting construction and initialization semantics. Therefore, a mixin must not inherit from any class other than the implicit `object`. Implementing interfaces is allowed and encouraged.

When a mixin inherits a base class, the generator reports `MXN002` and does not generate the corresponding `I_Mx*` interface.

## How to fix violations
- Remove the base class from the mixin declaration.
- If you need members from the former base class, extract them into an interface and implement that interface in the mixin, or copy the required members directly into the mixin.
- If inheritance is essential, reconsider whether this type should be a mixin at all.

## When to suppress warnings
Suppression is not recommended. This constraint is fundamental to predictable mixin composition. Prefer redesigning to interface-based contracts or composition.

## Example of a violation

### Description
`BaseThing` is a class and `TestMixinB` inherits it while being marked as a mixin.

### Code
```csharp
using MixinSharp;

namespace Samples;

public abstract class BaseThing
{
    protected int State;
}

[DefineMixin]
public abstract class TestMixinB : BaseThing // ❌ inherits a class (not allowed)
{
    public int GetState() => State;
}
```

## Example of how to fix

### Description
Remove the base class and, if needed, switch to interfaces.

### Code
```csharp
using MixinSharp;

namespace Samples;

public interface IHasState
{
    int GetState();
}

[DefineMixin]
public abstract class TestMixinB // ✅ no base class
    : IHasState // interfaces are fine
{
    private int _state;
    public int GetState() => _state;
}
```

## Related rules
[MXN001: Mixin class must be abstract](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN001_MixinClassMustBeAbstract.md)
[MXN004: UseMixin type must be a mixin](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN004_UseMixinTypeMustBeAMixin.md)