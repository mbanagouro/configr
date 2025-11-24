using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.Npgsql;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.Npgsql;

[Collection("NpgsqlIntegration")]
public sealed class NpgsqlIntegrationTests
{
    private async Task<DefaultConfigR> CreateSutAsync()
    {
        await NpgsqlTestDatabase.ClearTableAsync();

        var storeOptions = Options.Create(new NpgsqlConfigStoreOptions
        {
            ConnectionString = NpgsqlTestDatabase.GetConnectionString(),
            Schema = "public",
            Table = "configr",
            AutoCreateTable = true
        });

        var store = new NpgsqlConfigStore(storeOptions);
        var cache = new MemoryConfigCache();
        var serializer = new DefaultConfigSerializer();
        var formatter = new DefaultConfigKeyFormatter();
        var cfg = Options.Create(new ConfigROptions());

        return new DefaultConfigR(store, cache, serializer, formatter, cfg);
    }

    [Fact]
    public async Task Should_Save_And_Load_Config()
    {
        var configR = await CreateSutAsync();

        var expected = new SampleConfig
        {
            IntValue = 55,
            ShortValue = 3,
            LongValue = 9999999,
            Name = "postgresql",
            IsEnabled = true,
            CreatedAt = new DateTime(2025, 01, 01),
            Tags = ["pg", "configr"],
            Details = new NestedData { Level = 1, Description = "From Postgres" }
        };

        await configR.SaveAsync(expected);

        var loaded = await configR.GetAsync<SampleConfig>();

        loaded.IntValue.Should().Be(expected.IntValue);
        loaded.Name.Should().Be("postgresql");
        loaded.Details.Description.Should().Be("From Postgres");
    }
}
