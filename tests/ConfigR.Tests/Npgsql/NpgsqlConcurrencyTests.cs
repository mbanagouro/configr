using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.Npgsql;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.Npgsql;

[Collection("NpgsqlConcurrency")]
public sealed class NpgsqlConcurrencyTests
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
    public async Task ParallelReads_Should_Be_Consistent()
    {
        var sut = await CreateSutAsync();

        var initial = new SampleConfig
        {
            Name = "PgParallel",
            IntValue = 10,
            Details = new NestedData { Level = 1, Description = "PG Test" }
        };

        await sut.SaveAsync(initial);

        var tasks = Enumerable.Range(0, 100).Select(async _ =>
        {
            var cfg = await sut.GetAsync<SampleConfig>();
            cfg.Name.Should().Be("PgParallel");
            cfg.Details.Description.Should().Be("PG Test");
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
            Name = "CounterNpgsql",
            IsEnabled = true,
            Tags = new() { "start" },
            Details = new NestedData { Level = 1, Description = "Counter Npgsql" }
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
        finalCfg.Name.Should().Be("CounterNpgsql");
    }
}
