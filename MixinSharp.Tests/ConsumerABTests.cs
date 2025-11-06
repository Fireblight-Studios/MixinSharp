using System;
using System.Collections.Generic;
using MixinUtils.Sample;
using Xunit;

namespace MixinUtils.Tests;

public class ConsumerABTests
{

    [Fact]
    public static void Implements_MixinA_Members()
    {
        // Test Methods for MixinA

        var consumerMembers = MemberFetcher.GetMethods<TestConsumerAB>();
        var mixinMembers = MemberFetcher.GetMethods<TestMixinA>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }

    }

    [Fact]
    private static void Validate_MixinA_Properties()
    {
        // Test Properties for Mixin A
        
        var consumerMembers = MemberFetcher.GetProperties<TestConsumerAB>();
        var mixinMembers = MemberFetcher.GetProperties<TestMixinA>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }

    [Fact]
    private static void Validate_MixinA_Fields()
    {
        // Test Fields for Mixin A
        var consumerMembers = MemberFetcher.GetFields<TestConsumerAB>();
        var mixinMembers = MemberFetcher.GetFields<TestMixinA>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }


    [Fact]
    public static void Implements_MixinB_Members()
    {
        // Test Methods for Mixin B

        var consumerMembers = MemberFetcher.GetMethods<TestConsumerAB>();
        var mixinMembers = MemberFetcher.GetMethods<TestMixinB>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
        
    }

    [Fact]
    private static void Validate_MixinB_Fields()
    {
        // Test Fields for Mixin B
        var consumerMembers = MemberFetcher.GetFields<TestConsumerAB>();
        var mixinMembers = MemberFetcher.GetFields<TestMixinB>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }

    [Fact]
    private static void Validate_MixinB_Properties()
    {
        // Test Properties for Mixin B
        var consumerMembers = MemberFetcher.GetProperties<TestConsumerAB>();
        var mixinMembers = MemberFetcher.GetProperties<TestMixinB>();
        foreach (var mixinMember in mixinMembers)
        {
            Assert.Contains(mixinMember, consumerMembers);
        }
    }
    
    [Fact]
    public static void Inherits_IDisposable_From_MixinB()
    {
        
        // Sanity check, MixinB should implement IDisposable.

        var mixinType = typeof(TestMixinB);
        var disposableType = typeof(IDisposable);
        Assert.True(mixinType.IsAssignableTo(disposableType), "MixinB should be disposable");
        
        var consumerType = typeof(TestConsumerAB);
        Assert.True(consumerType.IsAssignableTo(disposableType), "TestConsumerAB is not assignable to IDisposable");
    }
}