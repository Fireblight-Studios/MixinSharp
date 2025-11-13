using System;
using MixinSharp;

namespace MixinUtils.Sample;

[UseMixin(typeof(TestMixinGeneric<Person>))]
public partial class TestConsumerPerson
{
    public int TimesBonked { get; set; } = 0;

    public void Bonk()
    {
        TimesBonked++;
        Console.WriteLine("Ouch!");
    }
}