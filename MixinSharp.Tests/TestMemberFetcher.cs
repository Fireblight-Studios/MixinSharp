using Xunit;

namespace MixinUtils.Tests;

public class TestMemberFetcher
{
    private class MemberFetcherSubject
    {
        public int FieldPublic;
        private string _fieldPrivate = string.Empty;

        public int PropPublic
        {
            get => FieldPublic;
            set => FieldPublic = value;
        }

        private string PropPrivate
        {
            get => _fieldPrivate;
            set => _fieldPrivate = value;
        }

        public void Foo() { }
        private int Bar(string s, int x) => x;
        protected internal string Baz(object o) => o?.ToString() ?? string.Empty;
    }

    [Fact]
    public void GetMethods_ReturnsExpectedSignatures()
    {
        var methods = MemberFetcher.GetMethods<MemberFetcherSubject>();

        Assert.Contains("System.Void Foo()", methods);
        Assert.Contains("System.Int32 Bar(System.String, System.Int32)", methods);
        Assert.Contains("System.String Baz(System.Object)", methods);
    }

    [Fact]
    public void GetProperties_ReturnsExpectedSignatures()
    {
        var props = MemberFetcher.GetProperties<MemberFetcherSubject>();

        Assert.Contains("System.Int32 PropPublic", props);
        Assert.Contains("System.String PropPrivate", props);
    }

    [Fact]
    public void GetFields_ReturnsExpectedSignatures()
    {
        var fields = MemberFetcher.GetFields<MemberFetcherSubject>();

        Assert.Contains("System.Int32 FieldPublic", fields);
        Assert.Contains("System.String _fieldPrivate", fields);
    }
}