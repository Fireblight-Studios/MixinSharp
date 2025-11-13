using MixinSharp;

namespace MixinUtils.Sample;

[DefineMixin]
public abstract class TestMixinGeneric<T> where T : class, new()
{
    public T? GenericProperty { get; set; } =  new T();

    public T MakeNewObject()
    {
        return new T();
    }
}