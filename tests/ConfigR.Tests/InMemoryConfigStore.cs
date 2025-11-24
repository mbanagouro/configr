using ConfigR.Abstractions;
using System.Collections.Concurrent;

namespace ConfigR.Tests;

public sealed class InMemoryConfigStore : IConfigStore
{
    private readonly ConcurrentDictionary<string, ConfigEntry> _data =
        new(StringComparer.OrdinalIgnoreCase);

    private static string FormatKey(string key, string? scope)
        => scope is null ? key : $"{scope}:{key}";

    public Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        var finalKey = FormatKey(key, scope);
        _data.TryGetValue(finalKey, out var value);
        return Task.FromResult<ConfigEntry?>(value);
    }

    public Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        if (scope is null)
        {
            // return everything
            return Task.FromResult<IReadOnlyDictionary<string, ConfigEntry>>(
                new Dictionary<string, ConfigEntry>(_data));
        }

        var prefix = $"{scope}:";
        var filtered = _data
            .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                kvp => kvp.Key.Substring(prefix.Length),
                kvp => kvp.Value,
                StringComparer.OrdinalIgnoreCase
            );

        return Task.FromResult<IReadOnlyDictionary<string, ConfigEntry>>(filtered);
    }

    public Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        foreach (var entry in entries)
        {
            var finalKey = FormatKey(entry.Key, scope);
            _data[finalKey] = entry;
        }
       
        return Task.CompletedTask;
    }
}
