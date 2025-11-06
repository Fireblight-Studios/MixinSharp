using MixinSharp;

namespace MixinUtils.Sample;

[UseMixin(typeof(TestMixinA))]
[UseMixin(typeof(TestMixinB))]
public partial class TestConsumerAB
{
    public bool ImplementMe()
    {
        return true;
    }
}