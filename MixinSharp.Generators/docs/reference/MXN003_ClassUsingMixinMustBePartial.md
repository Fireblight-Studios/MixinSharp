# MXN003: Class using mixin must be partial

## Cause
A class annotated with `[UseMixin]` is not declared `partial`.

## Rule description
The generator augments classes that use one or more mixins by emitting an additional partial class with copied members and added implemented interfaces. For this to compile, the original class must be declared `partial`. If a non‑partial class uses `[UseMixin]`, the generator reports `MXN003` and skips augmentation.

## How to fix violations
- Add the `partial` modifier to the class that has the `[UseMixin]` attribute(s).

## When to suppress warnings
Do not suppress. The generator needs a partial declaration to merge generated members with your source. If you do not want mixin augmentation, remove `[UseMixin]`.

## Example of a violation

### Description
`TestConsumer` uses a mixin but is not declared partial.

### Code
```csharp
using MixinSharp;

namespace Samples;

[DefineMixin]
public abstract class LoggerMixin
{
    public void Log(string message) { /* ... */ }
}

[UseMixin(typeof(LoggerMixin))]
public class TestConsumer // ❌ not partial
{
}
```

## Example of how to fix

### Description
Declare the consumer class as `partial`.

### Code
```csharp
using MixinSharp;

namespace Samples;

[DefineMixin]
public abstract class LoggerMixin
{
    public void Log(string message) { /* ... */ }
}

[UseMixin(typeof(LoggerMixin))]
public partial class TestConsumer // ✅ partial
{
}
```

## Related rules
[MXN004: UseMixin type must be a mixin](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN004_UseMixinTypeMustBeAMixin.md)
[MXN005: Member conflict when applying mixin](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN005_MemberConflictWhenApplyingMixin.md)