using System;
using MixinSharp;

namespace MixinUtils.Sample;
[DefineMixin]
public abstract class TestMixinA
{
    public int TestProperty { get; set; } = 2;
    public string TestStringProperty { get; set; } = "Hello World";
    private string _secretProperty= String.Empty;

    public string GetSecretProperty()
    {
        return _secretProperty;
    }

    public void SetSecretProperty(string secretProperty)
    {
        _secretProperty = secretProperty;
    }
    
    private int SecretMethod()
    {
        return 42;
    }

    public int GetSecretMethod()
    {
        return SecretMethod();
    }

    public abstract bool ImplementMe();
    
    public virtual bool OverrideMe()
    {
        return true;
    }
}