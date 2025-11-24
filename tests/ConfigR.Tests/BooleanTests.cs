using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests;

public class BooleanTests
{
    private readonly IConfigR _config;

    public BooleanTests()
    {
        _config = new DefaultConfigR(new InMemoryConfigStore(),
            new MemoryConfigCache(), 
            new DefaultConfigSerializer(), 
            new DefaultConfigKeyFormatter(),
            Options.Create(new ConfigROptions()));
    }

    [Fact]
    public async Task Should_Save_And_Load_Boolean()
    {
        var cfg = new SampleConfig { IsEnabled = false };

        await _config.SaveAsync(cfg);
        var loaded = await _config.GetAsync<SampleConfig>();

        loaded.IsEnabled.Should().BeFalse();
    }
}
