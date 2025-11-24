using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests;

public class NumericTypeTests
{
    private readonly IConfigR _config;

    public NumericTypeTests()
    {
        _config = new DefaultConfigR(new InMemoryConfigStore(),
            new MemoryConfigCache(),
            new DefaultConfigSerializer(),
            new DefaultConfigKeyFormatter(),
            Options.Create(new ConfigROptions()));
    }

    [Fact]
    public async Task Should_Save_And_Load_All_Numeric_Types()
    {
        var cfg = new SampleConfig
        {
            IntValue = 42,
            ShortValue = 7,
            LongValue = 1234567890123
        };

        await _config.SaveAsync(cfg);
        var loaded = await _config.GetAsync<SampleConfig>();

        loaded.IntValue.Should().Be(42);
        loaded.ShortValue.Should().Be((short)7);
        loaded.LongValue.Should().Be(1234567890123);
    }
}
