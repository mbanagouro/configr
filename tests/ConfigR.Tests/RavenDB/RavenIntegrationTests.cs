using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.RavenDB;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.RavenDB;

[Collection("RavenIntegration")]
public sealed class RavenIntegrationTests
{
    private async Task<DefaultConfigR> CreateSutAsync()
    {
        await RavenTestDatabase.ClearDatabaseAsync().ConfigureAwait(false);

        var documentStore = await RavenTestDatabase.GetDocumentStoreAsync().ConfigureAwait(false);

        var storeOptions = Options.Create(new RavenDbConfigStoreOptions
        {
            Urls = RavenTestDatabase.GetUrls(),
            Database = RavenTestDatabase.GetDatabaseName(),
            KeyPrefix = "configr-integration-tests"
        });

        var store = new RavenDbConfigStore(documentStore, storeOptions);
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
            IntValue = 222,
            ShortValue = 7,
            LongValue = 9876543210,
            Name = "RavenDBTest",
            IsEnabled = true,
            CreatedAt = new DateTime(2024, 12, 1),
            Tags = new() { "ravendb", "integration" },
            Details = new NestedData { Level = 5, Description = "RavenDB" }
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
