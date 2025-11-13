using MixinUtils.Sample;
using Xunit;

namespace MixinUtils.Tests;

public static class ConsumerGenericTests
{
    private class Foo
    {
        private int Bar { get; set; } = 0;
    }
    [Fact]
    public static void ConfirmMethodsTest()
    {
        var consumerMembers = MemberFetcher.GetMethods<TestConsumerPerson>();
        var mixinMembers = MemberFetcher.GetMethods<TestMixinGeneric<Person>>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }
    
    [Fact]
    public static void ConfirmPropertiesTest()
    {
        var consumerMembers = MemberFetcher.GetProperties<TestConsumerPerson>();
        var mixinMembers = MemberFetcher.GetProperties<TestMixinGeneric<Person>>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }

    [Fact]
    public static void ConfirmTypeTest()
    {
        var personType = typeof(Person);
        var consumerType = typeof(TestConsumerPerson);
        var mixinType = typeof(TestMixinGeneric<Person>);
        
        Assert.Equal(personType, consumerType.GetProperty(nameof(TestConsumerPerson.GenericProperty))?.PropertyType);
        Assert.Equal(personType, mixinType.GetProperty(nameof(TestMixinGeneric<Person>.GenericProperty))?.PropertyType);
    }
    
    
    [Fact]
    public static void ConfirmMethodsGenericsTest()
    {
        var consumerMembers = MemberFetcher.GetMethods<TestConsumerGeneric<Foo>>();
        var mixinMembers = MemberFetcher.GetMethods<TestMixinGeneric<Foo>>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }
    
    [Fact]
    public static void ConfirmPropertiesGenericsTest()
    {
        var consumerMembers = MemberFetcher.GetProperties<TestConsumerGeneric<Foo>>();
        var mixinMembers = MemberFetcher.GetProperties<TestMixinGeneric<Foo>>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }

    [Fact]
    public static void ConfirmTypeGenericsTest()
    {
        var personType = typeof(Foo);
        var consumerType = typeof(TestConsumerGeneric<Foo>);
        var mixinType = typeof(TestMixinGeneric<Foo>);
        
        Assert.Equal(personType, consumerType.GetProperty(nameof(TestConsumerGeneric<Foo>.GenericProperty))?.PropertyType);
        Assert.Equal(personType, mixinType.GetProperty(nameof(TestMixinGeneric<Foo>.GenericProperty))?.PropertyType);
    }
    
}