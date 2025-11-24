using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests;

public class FullConfigTests
{
    private readonly IConfigR _config;

    public FullConfigTests()
    {
        _config = new DefaultConfigR(new InMemoryConfigStore(),
            new MemoryConfigCache(),
            new DefaultConfigSerializer(),
            new DefaultConfigKeyFormatter(),
            Options.Create(new ConfigROptions()));
    }

    [Fact]
    public async Task Should_Save_And_Load_All_Types_Correctly()
    {
        var expected = new SampleConfig
        {
            IntValue = 123,
            ShortValue = 9,
            LongValue = 9999999999,
            Name = "Test",
            IsEnabled = true,
            CreatedAt = new DateTime(2024, 11, 20),
            Tags = new() { "one", "two", "three" },
            Details = new NestedData { Level = 5, Description = "Nested" }
        };

        await _config.SaveAsync(expected);
        var loaded = await _config.GetAsync<SampleConfig>();

        loaded.IntValue.Should().Be(expected.IntValue);
        loaded.ShortValue.Should().Be(expected.ShortValue);
        loaded.LongValue.Should().Be(expected.LongValue);
        loaded.Name.Should().Be(expected.Name);
        loaded.CreatedAt.Should().Be(expected.CreatedAt);
        loaded.IsEnabled.Should().Be(expected.IsEnabled);
        loaded.Tags.Should().Equal(expected.Tags);
        loaded.Details.Level.Should().Be(expected.Details.Level);
        loaded.Details.Description.Should().Be(expected.Details.Description);
    }
}
