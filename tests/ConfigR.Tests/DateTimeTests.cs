using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests;

public class DateTimeTests
{
    private readonly IConfigR _config;

    public DateTimeTests()
    {
        _config = new DefaultConfigR(new InMemoryConfigStore(),
            new MemoryConfigCache(),
            new DefaultConfigSerializer(),
            new DefaultConfigKeyFormatter(),
            Options.Create(new ConfigROptions()));
    }

    [Fact]
    public async Task Should_Save_And_Load_DateTime()
    {
        var date = new DateTime(2025, 4, 1);
        var cfg = new SampleConfig { CreatedAt = date };

        await _config.SaveAsync(cfg);
        var loaded = await _config.GetAsync<SampleConfig>();

        loaded.CreatedAt.Should().Be(date);
    }
}
