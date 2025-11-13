using MixinSharp;

namespace MixinUtils.Sample;

[UseMixin(typeof(TestMixinGeneric<>))]
public partial class TestConsumerGeneric<T>
{
    public T SisterProperty { get; set; } = new T();
}