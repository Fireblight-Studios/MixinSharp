using System;
using MixinSharp;

namespace MixinUtils.Sample;

[DefineMixin]
public abstract class TestMixinB : IDisposable
{
    public int TimesDisposed { get; set; } = 0;

    public void Dispose()
    {
        TimesDisposed++;
    }
}