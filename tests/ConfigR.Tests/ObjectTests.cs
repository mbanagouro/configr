using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests;

public class ObjectTests
{
    private readonly IConfigR _config;

    public ObjectTests()
    {
        _config = new DefaultConfigR(new InMemoryConfigStore(),
            new MemoryConfigCache(),
            new DefaultConfigSerializer(),
            new DefaultConfigKeyFormatter(),
            Options.Create(new ConfigROptions()));
    }

    [Fact]
    public async Task Should_Save_And_Load_Nested_Object()
    {
        var cfg = new SampleConfig
        {
            Details = new NestedData { Level = 77, Description = "Nested OK" }
        };

        await _config.SaveAsync(cfg);
        var loaded = await _config.GetAsync<SampleConfig>();

        loaded.Details.Level.Should().Be(77);
        loaded.Details.Description.Should().Be("Nested OK");
    }
}
