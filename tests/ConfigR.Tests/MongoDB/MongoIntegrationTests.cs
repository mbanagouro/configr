using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.MongoDB;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.MongoDB;

[Collection("MongoIntegration")]
public sealed class MongoIntegrationTests
{
    private async Task<DefaultConfigR> CreateSutAsync()
    {
        await MongoTestDatabase.ClearCollectionAsync().ConfigureAwait(false);

        var storeOptions = Options.Create(new MongoConfigStoreOptions
        {
            ConnectionString = MongoTestDatabase.GetConnectionString(),
            Database = MongoTestDatabase.GetDatabaseName(),
            Collection = "ConfigR_IntegrationTests"
        });

        var store = new MongoConfigStore(storeOptions);
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
            IntValue = 777,
            ShortValue = 12,
            LongValue = 1234567890,
            Name = "MongoTest",
            IsEnabled = true,
            CreatedAt = new DateTime(2025, 1, 1),
            Tags = new() { "mongo", "integration" },
            Details = new NestedData { Level = 3, Description = "Mongo DB" }
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
