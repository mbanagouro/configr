using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.Redis;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.Redis;

[Collection("RedisIntegration")]
public class RedisConcurrencyTests
{
    private async Task<DefaultConfigR> CreateSutAsync()
    {
        await RedisTestDatabase.FlushAsync();

        var mux = await RedisTestDatabase.GetConnectionAsync();

        var storeOptions = Options.Create(new RedisConfigStoreOptions
        {
            ConnectionString = "localhost:6379",
            KeyPrefix = "configr-concurrency-tests"
        });

        var store = new RedisConfigStore(mux, storeOptions);
        var cache = new MemoryConfigCache();
        var serializer = new DefaultConfigSerializer();
        var formatter = new DefaultConfigKeyFormatter();
        var cfg = Options.Create(new ConfigROptions());

        return new DefaultConfigR(store, cache, serializer, formatter, cfg);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task ParallelReads_Should_Be_Consistent()
    {
        var configR = await CreateSutAsync().ConfigureAwait(false);

        var initial = new SampleConfig
        {
            IntValue = 10,
            Name = "ParallelRedis",
            IsEnabled = true,
            Tags = new() { "one", "two" },
            Details = new NestedData { Level = 1, Description = "Parallel Redis" }
        };

        await configR.SaveAsync(initial).ConfigureAwait(false);

        var tasks = Enumerable.Range(0, 100)
            .Select(async _ =>
            {
                var cfg = await configR.GetAsync<SampleConfig>().ConfigureAwait(false);
                cfg.Should().NotBeNull();
                cfg.Name.Should().Be("ParallelRedis");
                cfg.Details.Should().NotBeNull();
                cfg.Details.Description.Should().Be("Parallel Redis");
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
            Name = "CounterRedis",
            IsEnabled = true,
            Tags = new() { "start" },
            Details = new NestedData { Level = 1, Description = "Counter Redis" }
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
        finalCfg.Name.Should().Be("CounterRedis");
    }
}
