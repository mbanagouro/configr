using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.Core;

public sealed class DefaultConfigRTests
{
    private sealed class InMemoryStore : IConfigStore
    {
        private readonly Dictionary<string, ConfigEntry> _data = new(StringComparer.OrdinalIgnoreCase);

        public Task<ConfigEntry?> GetAsync(string key, string? scope = null)
        {
            _data.TryGetValue(key, out var entry);
            return Task.FromResult<ConfigEntry?>(entry);
        }

        public Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
        {
            return Task.FromResult<IReadOnlyDictionary<string, ConfigEntry>>(_data);
        }

        public Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
        {
            foreach (var entry in entries)
            {
                _data[entry.Key] = entry;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class CheckoutConfig
    {
        public bool LoginRequired { get; set; }
        public int MaxItems { get; set; } = 10;
    }

    private static DefaultConfigR CreateSut(IConfigStore store)
    {
        var cache = new MemoryConfigCache();
        var serializer = new DefaultConfigSerializer();
        var keyFormatter = new DefaultConfigKeyFormatter();
        var options = Options.Create(new ConfigROptions());

        return new DefaultConfigR(store, cache, serializer, keyFormatter, options);
    }

    [Fact]
    public async Task SaveAndGet_Roundtrip_Works()
    {
        var store = new InMemoryStore();
        var configR = CreateSut(store);

        var original = new CheckoutConfig
        {
            LoginRequired = false,
            MaxItems = 42
        };

        await configR.SaveAsync(original);

        var loaded = await configR.GetAsync<CheckoutConfig>();

        loaded.LoginRequired.Should().BeFalse();
        loaded.MaxItems.Should().Be(42);
    }

    [Fact]
    public async Task Get_UsesCacheAfterFirstLoad()
    {
        var store = new InMemoryStore();
        var configR = CreateSut(store);

        var config1 = await configR.GetAsync<CheckoutConfig>();

        config1.Should().NotBeNull();

        await store.UpsertAsync(new[]
        {
            new ConfigEntry { Key = "checkoutconfig.loginrequired", Value = "false" }
        });

        var config2 = await configR.GetAsync<CheckoutConfig>();

        config2.LoginRequired.Should().Be(config1.LoginRequired);
    }
}
