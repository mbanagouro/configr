using ConfigR.Abstractions;
using ConfigR.Core;
using FluentAssertions;
using Xunit;

namespace ConfigR.Tests.Core;

public sealed class MemoryCacheTests
{
    [Fact]
    public void Cache_SetAndGet_WorksPerScope()
    {
        var cache = new MemoryConfigCache();
        var scope1 = "s1";
        var scope2 = "s2";

        var entries1 = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };
        var entries2 = new Dictionary<string, ConfigEntry>
        {
            ["k2"] = new() { Key = "k2", Value = "v2" }
        };

        cache.SetAll(scope1, entries1, TimeSpan.FromMinutes(10));
        cache.SetAll(scope2, entries2, TimeSpan.FromMinutes(10));

        cache.TryGetAll(scope1, out var result1).Should().BeTrue();
        cache.TryGetAll(scope2, out var result2).Should().BeTrue();

        result1.Should().ContainKey("k1");
        result2.Should().ContainKey("k2");
    }

    [Fact]
    public void Cache_Clear_RemovesScope()
    {
        var cache = new MemoryConfigCache();
        var entries = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };

        cache.SetAll("scope", entries, TimeSpan.FromMinutes(10));
        cache.Clear("scope");

        cache.TryGetAll("scope", out _).Should().BeFalse();
    }

    [Fact]
    public void Cache_ClearAll_RemovesAll()
    {
        var cache = new MemoryConfigCache();
        var entries = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };

        cache.SetAll("s1", entries, TimeSpan.FromMinutes(10));
        cache.SetAll("s2", entries, TimeSpan.FromMinutes(10));
        cache.ClearAll();

        cache.TryGetAll("s1", out _).Should().BeFalse();
        cache.TryGetAll("s2", out _).Should().BeFalse();
    }

    [Fact]
    public void Cache_WithNullDuration_DoesNotCache()
    {
        var cache = new MemoryConfigCache();
        var entries = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };

        cache.SetAll("scope", entries, cacheDuration: null);

        cache.TryGetAll("scope", out _).Should().BeFalse();
    }

    [Fact]
    public void Cache_WithZeroDuration_DoesNotCache()
    {
        var cache = new MemoryConfigCache();
        var entries = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };

        cache.SetAll("scope", entries, TimeSpan.Zero);

        cache.TryGetAll("scope", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Cache_ExpiredEntry_ReturnsNotFound()
    {
        var cache = new MemoryConfigCache();
        var entries = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };

        // Cache com duração muito curta
        cache.SetAll("scope", entries, TimeSpan.FromMilliseconds(50));

        // Aguarda expirar
        await Task.Delay(100);

        cache.TryGetAll("scope", out _).Should().BeFalse();
    }

    [Fact]
    public void Cache_NotExpiredEntry_ReturnsFound()
    {
        var cache = new MemoryConfigCache();
        var entries = new Dictionary<string, ConfigEntry>
        {
            ["k1"] = new() { Key = "k1", Value = "v1" }
        };

        cache.SetAll("scope", entries, TimeSpan.FromMinutes(10));

        cache.TryGetAll("scope", out var result).Should().BeTrue();
        result.Should().ContainKey("k1");
        result["k1"].Value.Should().Be("v1");
    }
}
