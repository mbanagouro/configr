using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests;

public class CollectionTests
{
    private readonly IConfigR _config;

    public CollectionTests()
    {
        _config = new DefaultConfigR(new InMemoryConfigStore(),
            new MemoryConfigCache(),
            new DefaultConfigSerializer(),
            new DefaultConfigKeyFormatter(),
            Options.Create(new ConfigROptions()));
    }

    [Fact]
    public async Task Should_Save_And_Load_Collections()
    {
        var cfg = new SampleConfig
        {
            Tags = new List<string> { "aspnet", "configr", "leanwork" }
        };

        await _config.SaveAsync(cfg);
        var loaded = await _config.GetAsync<SampleConfig>();

        loaded.Tags.Should().Contain("configr");
        loaded.Tags.Should().HaveCount(3);
    }
}
