using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.MongoDB;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.MongoDB;

[Collection("MongoConcurrency")]
public sealed class MongoConcurrencyTests
{
    private async Task<DefaultConfigR> CreateSutAsync()
    {
        await MongoTestDatabase.ClearCollectionAsync().ConfigureAwait(false);

        var storeOptions = Options.Create(new MongoConfigStoreOptions
        {
            ConnectionString = MongoTestDatabase.GetConnectionString(),
            Database = MongoTestDatabase.GetDatabaseName(),
            Collection = "ConfigR_ConcurrencyTests"
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
    public async Task ParallelReads_Should_Be_Consistent()
    {
        var configR = await CreateSutAsync().ConfigureAwait(false);

        var initial = new SampleConfig
        {
            IntValue = 10,
            Name = "ParallelMongo",
            IsEnabled = true,
            Tags = new() { "one", "two" },
            Details = new NestedData { Level = 1, Description = "Parallel Mongo" }
        };

        await configR.SaveAsync(initial).ConfigureAwait(false);

        var tasks = Enumerable.Range(0, 100)
            .Select(async _ =>
            {
                var cfg = await configR.GetAsync<SampleConfig>().ConfigureAwait(false);
                cfg.Should().NotBeNull();
                cfg.Name.Should().Be("ParallelMongo");
                cfg.Details.Should().NotBeNull();
                cfg.Details.Description.Should().Be("Parallel Mongo");
            });

        await Task.WhenAll(tasks);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task ParallelWriteRead_Should_Not_Corrupt_State()
    {
        var configR = await CreateSutAsync().ConfigureAwait(false);

        var initial = new SampleConfig
        {
            IntValue = 0,
            Name = "CounterMongo",
            IsEnabled = true,
            Tags = new() { "start" },
            Details = new NestedData { Level = 1, Description = "Counter Mongo" }
        };

        await configR.SaveAsync(initial).ConfigureAwait(false);

        var tasks = Enumerable.Range(0, 50)
            .Select(async _ =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var cfg = await configR.GetAsync<SampleConfig>().ConfigureAwait(false);
                    cfg.Tags.Should().NotBeNull();
                    cfg.Details.Should().NotBeNull();

                    cfg.IntValue++;
                    await configR.SaveAsync(cfg).ConfigureAwait(false);
                }
            });

        await Task.WhenAll(tasks);

        var finalCfg = await configR.GetAsync<SampleConfig>().ConfigureAwait(false);
        finalCfg.Tags.Should().NotBeNull();
        finalCfg.Details.Should().NotBeNull();
        finalCfg.Name.Should().Be("CounterMongo");
    }
}
