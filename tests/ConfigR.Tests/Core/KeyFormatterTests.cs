using System;
using ConfigR.Core;
using FluentAssertions;
using Xunit;

namespace ConfigR.Tests.Core;

public sealed class KeyFormatterTests
{
    private readonly DefaultConfigKeyFormatter _formatter = new();

    private sealed class CheckoutConfig
    {
        public bool LoginRequired { get; set; }
    }

    [Fact]
    public void GetKey_Formats_ClassAndProperty_Lowercase()
    {
        var key = _formatter.GetKey(typeof(CheckoutConfig), nameof(CheckoutConfig.LoginRequired));
        key.Should().Be("checkoutconfig.loginrequired");
    }

    [Fact]
    public void Normalize_TrimsAndLowerCases()
    {
        var key = _formatter.Normalize("  Test.KEY ");
        key.Should().Be("test.key");
    }

    [Fact]
    public void GetKey_NullType_Throws()
    {
        Action act = () => _formatter.GetKey(null!, "Prop");
        act.Should().Throw<ArgumentNullException>();
    }
}
