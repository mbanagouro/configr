using ConfigR.Abstractions;
using ConfigR.Core;
using ConfigR.Tests;
using FluentAssertions;
using Xunit;

namespace ConfigR.Tests;

public sealed class ConcurrencyTests
{
    private DefaultConfigR CreateSut()
    {
        var store = new InMemoryConfigStore();
        var cache = new MemoryConfigCache();
        var serializer = new DefaultConfigSerializer();
        var keyFormatter = new DefaultConfigKeyFormatter();
        var options = Microsoft.Extensions.Options.Options.Create(new ConfigROptions());
        return new DefaultConfigR(store, cache, serializer, keyFormatter, options);
    }

    [Fact]
    public async Task ParallelReads_Should_NotThrow_And_Return_Consistent_Data()
    {
        var configR = CreateSut();

        var initial = new SampleConfig
        {
            IntValue = 10,
            Name = "Parallel",
            IsEnabled = true,
            Tags = new() { "one", "two" },
            Details = new NestedData { Level = 2, Description = "Parallel" }
        };

        await configR.SaveAsync(initial);

        var tasks = Enumerable.Range(0, 100)
            .Select(async _ =>
            {
                var cfg = await configR.GetAsync<SampleConfig>();
                cfg.Should().NotBeNull();
                cfg.Name.Should().Be("Parallel");
                cfg.Details.Should().NotBeNull();
                cfg.Details.Description.Should().Be("Parallel");
            });

        await Task.WhenAll(tasks);
    }

    [Fact]
    public async Task ParallelWriteRead_Should_Not_Corrupt_State()
    {
        var configR = CreateSut();

        var initial = new SampleConfig
        {
            IntValue = 0,
            Name = "Counter",
            IsEnabled = true,
            Tags = new() { "start" },
            Details = new NestedData { Level = 1, Description = "Counter" }
        };

        await configR.SaveAsync(initial);

        var tasks = Enumerable.Range(0, 50)
            .Select(async _ =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var cfg = await configR.GetAsync<SampleConfig>();
                    cfg.Tags.Should().NotBeNull();
                    cfg.Details.Should().NotBeNull();

                    cfg.IntValue++;
                    await configR.SaveAsync(cfg);
                }
            });

        await Task.WhenAll(tasks);

        var finalCfg = await configR.GetAsync<SampleConfig>();
        finalCfg.Tags.Should().NotBeNull();
        finalCfg.Details.Should().NotBeNull();
        finalCfg.Name.Should().Be("Counter");
    }
}
