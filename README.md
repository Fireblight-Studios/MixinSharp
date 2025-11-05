# MixinSharp

MixinSharp is a tool that adds the power and flexibility of Mixins to C#! Traditionally, C# does not natively support
mixins, but with the magic of Roslyn source generators, this library allows you to designate an abstract class as with
an Attribute, and consume it in any partial class to implement Mixin behavior. Interfaces are also automatically created
from all of a mixin's public members.

## What is a Mixin?

Think of a mixin as like a piece of a class that is consumed in other classes, but not inherited. Mixins implement 
behaviors through shared implementation and are used in some object-oriented languages to get around the 
Inheritance Diamond of Death. Some languages will refer to mixins as traits.

## What about Interfaces?

Interfaces only define a "contract", not an implementation of that contract. It is the responsibility of the implementing
class to implement **all** members of that interface. However, a mixin is both a contract and a partial or even
full implementation of that contract. This ends up being advantageous since often times classes that implement an
interface will have very similar behaviors / implementations.

C# has addressed some of this over the years by adding things like extension methods or default implementations for
some interface members. However, these approaches have limitations, such as these methods stuck being static, or only
having access to a class's public members, no instance fields, or these default implementations not being available from
within the implementing class without the use of casting.

## How does MixinSharp help?

Mixins are meant to be complete, or near complete implementations of a contract. Starting with an abstract class, a set
of behaviors are defined through standard instance methods, fields, and properties. These may be public, private,
protected, etc... Or can be abstract, virtual, partial, etc... Just like a normal abstract class would. The only difference
is that we apply a `[MixinDefinition]` attribute to the class when declared. Then when the class is compiled, an interface
is generated from all public facing members of the mixin class.

Consuming classes then consume one or more mixins using a `[UseMixin]` attribute, and are injected into the consuming
class during compilation, minus any abstract members. These must still be defined by the consuming class.

## What are the limitations?

In order for a class to qualify as a mixin, it must meet the following criteria:

 - The class **MUST** be abstract.
 - The class can **NOT** inherit from another class / have a parent.
 - The class **MAY** be / use generics.
 - The class **MAY** use or implement interfaces.
 - The class **MAY** use virtual members to allow consumers to override them.
 - The class **MAY** use abstract members, these will be applied to the created interface.
 - The class **MAY** tag one of its methods as a pseudo constructor to perform setup.

In addition, consuming classes must also follow the following rules:

 - The class **MUST** be partial.
 - If the consumed mixin designates a pseudo constructor, the consuming class **MUST** call it in its constructor. This is enforced at compile time.
 - The class **MUST** implement any members that were marked as abstract on the mixin being consumed.
 - The class may **NOT** contain any members with the same name as any of the consumed Mixins, this is also checked and enforced at compile time.
