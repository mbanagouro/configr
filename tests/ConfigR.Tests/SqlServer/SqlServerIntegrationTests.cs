using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.SqlServer;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.SqlServer;

[Collection("SqlServerIntegration")]
public sealed class SqlServerIntegrationTests
{
    private async Task<DefaultConfigR> CreateSutAsync()
    {
        await SqlServerTestDatabase.EnsureDatabaseAndTableAsync().ConfigureAwait(false);
        await SqlServerTestDatabase.ClearTableAsync().ConfigureAwait(false);

        var storeOptions = Options.Create(new SqlServerConfigStoreOptions
        {
            ConnectionString = SqlServerTestDatabase.GetConnectionString(),
            Schema = "dbo",
            Table = "ConfigR_IntegrationTests",
            AutoCreateTable = true
        });

        var store = new SqlServerConfigStore(storeOptions);
        var cache = new MemoryConfigCache();
        var serializer = new DefaultConfigSerializer();
        var keyFormatter = new DefaultConfigKeyFormatter();
        var cfgOptions = Options.Create(new ConfigROptions());

        return new DefaultConfigR(store, cache, serializer, keyFormatter, cfgOptions);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task Should_Save_And_Load_Config()
    {
        var configR = await CreateSutAsync().ConfigureAwait(false);

        var expected = new SampleConfig
        {
            IntValue = 321,
            ShortValue = 3,
            LongValue = 9876543210,
            Name = "FromSqlServer",
            IsEnabled = false,
            CreatedAt = new DateTime(2024, 10, 1),
            Tags = new() { "sql", "integration" },
            Details = new NestedData { Level = 9, Description = "From SQL" }
        };

        await configR.SaveAsync(expected).ConfigureAwait(false);
        var loaded = await configR.GetAsync<SampleConfig>().ConfigureAwait(false);

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