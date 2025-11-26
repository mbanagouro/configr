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
            Table = "configr_integration_tests",
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task Should_Save_And_Load_Config()
    {
        var configR = await CreateSutAsync().ConfigureAwait(false);

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

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task Should_Override_Existing_Key_On_Upsert()
    {
        var configR = await CreateSutAsync().ConfigureAwait(false);

        var cfg = new SampleConfig
        {
            IntValue = 1,
            Name = "First"
        };

        await configR.SaveAsync(cfg).ConfigureAwait(false);

        cfg.IntValue = 2;
        cfg.Name = "Second";

        await configR.SaveAsync(cfg).ConfigureAwait(false);

        var loaded = await configR.GetAsync<SampleConfig>().ConfigureAwait(false);

        loaded.IntValue.Should().Be(2);
        loaded.Name.Should().Be("Second");
    }
}
