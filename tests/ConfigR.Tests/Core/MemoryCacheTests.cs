using System.Collections.Generic;
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

        cache.SetAll(scope1, entries1);
        cache.SetAll(scope2, entries2);

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

        cache.SetAll("scope", entries);
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

        cache.SetAll("s1", entries);
        cache.SetAll("s2", entries);
        cache.ClearAll();

        cache.TryGetAll("s1", out _).Should().BeFalse();
        cache.TryGetAll("s2", out _).Should().BeFalse();
    }
}
