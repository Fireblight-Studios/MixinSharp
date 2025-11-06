# MXN005: Member conflict when applying mixin

## Cause
A consumer class already contains a member with the same name as a non-abstract member provided by a used mixin.

## Rule description
When a class is annotated with `[UseMixin]`, the generator copies eligible members from the referenced mixin types into the consumer. If the consumer already defines a member with the same name, the generator cannot safely copy the mixin member and reports `MXN005`.

Important details:
- Abstract members declared in mixins are ignored by the conflict check because they are intended to be implemented by the consumer; they do not trigger `MXN005`.
- Static members are not copied and therefore do not participate in conflicts.
- Constructors are not copied.

## How to fix violations
- Rename the conflicting member on the consumer or the mixin so that names no longer collide.
- Remove the duplicate member if it is unnecessary.
- If the mixin member represents a contract that the consumer should fulfill, consider making it `abstract` in the mixin and implementing it on the consumer (no conflict in that case).

## When to suppress warnings
Suppress only if you fully understand that the mixin member will not be copied due to the conflict and you accept the resulting behavior. Generally, prefer resolving the conflict to ensure expected mixin functionality is available.

## Example of a violation

### Description
The consumer `TestConsumer` already has a `Dispose()` method, and the mixin provides a non-abstract `Dispose()` with the same name.

### Code
```csharp
using System;
using MixinSharp;

namespace Samples;

[DefineMixin]
public abstract class DisposableMixin : IDisposable
{
    // Non-abstract member that will be copied unless there is a conflict
    public void Dispose() { /* ... */ }
}

[UseMixin(typeof(DisposableMixin))]
public partial class TestConsumer : IDisposable
{
    // ❌ Conflict: same name as mixin member. MXN005 will be reported.
    public void Dispose() { /* consumer's own implementation */ }
}
```

## Example of how to fix

### Description
Rename the consumer member, or make the mixin member abstract so the consumer provides it.

### Code
```csharp
using System;
using MixinSharp;

namespace Samples;

[DefineMixin]
public abstract class DisposableMixin : IDisposable
{
    // Option A: keep non-abstract and ensure consumer has no conflicting member
    public void Dispose() { /* ... */ }
}

[UseMixin(typeof(DisposableMixin))]
public partial class TestConsumer : IDisposable
{
    // ✅ No conflict: removed/renamed the consumer method so the mixin member can be copied
    // public void Dispose() { }
}

// OR

[DefineMixin]
public abstract class ContractDisposableMixin : IDisposable
{
    // Option B: make it abstract; consumer must implement, and no MXN005 is raised
    public abstract void Dispose();
}

[UseMixin(typeof(ContractDisposableMixin))]
public partial class TestConsumer2 : IDisposable
{
    public void Dispose() { /* required by abstract mixin member */ }
}
```

## Related rules
[MXN003: Class using mixin must be partial](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN003_ClassUsingMixinMustBePartial.md)
[MXN004: UseMixin type must be a mixin](https://github.com/Fireblight-Studios/MixinSharp/blob/master/MixinSharp.Generators/docs/reference/MXN004_UseMixinTypeMustBeAMixin.md)