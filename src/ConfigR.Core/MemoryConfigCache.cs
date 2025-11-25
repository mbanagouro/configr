using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ConfigR.Abstractions;

namespace ConfigR.Core;

/// <summary>
/// In-memory implementation of configuration caching.
/// </summary>
public sealed class MemoryConfigCache : IConfigCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, ConfigEntry>> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Attempts to retrieve all cached configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The scope key.</param>
    /// <param name="entries">The cached configuration entries if found.</param>
    /// <returns>True if the entries were found in the cache; otherwise, false.</returns>
    public bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries)
    {
        scope ??= string.Empty;
        return _cache.TryGetValue(scope, out entries!);
    }

    /// <summary>
    /// Caches all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The scope key.</param>
    /// <param name="entries">The configuration entries to cache.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> is null.</exception>
    public void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries)
    {
        scope ??= string.Empty;

        ArgumentNullException.ThrowIfNull(entries);

        _cache[scope] = entries;
    }

    /// <summary>
    /// Clears the cache for a specific scope.
    /// </summary>
    /// <param name="scope">The scope key to clear.</param>
    public void Clear(string scope)
    {
        scope ??= string.Empty;
        _cache.TryRemove(scope, out _);
    }

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    public void ClearAll()
    {
        _cache.Clear();
    }
}
