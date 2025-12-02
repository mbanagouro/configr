using ConfigR.Abstractions;
using System.Collections.Concurrent;

namespace ConfigR.Core;

/// <summary>
/// In-memory implementation of configuration caching with configurable expiration.
/// </summary>
public sealed class MemoryConfigCache : IConfigCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    private sealed class CacheEntry
    {
        public required IReadOnlyDictionary<string, ConfigEntry> Entries { get; init; }
        public DateTime ExpiresAt { get; init; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Attempts to retrieve all cached configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The scope key.</param>
    /// <param name="entries">The cached configuration entries if found.</param>
    /// <returns>True if the entries were found in the cache and are not expired; otherwise, false.</returns>
    public bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries)
    {
        scope ??= string.Empty;
        entries = null!;

        if (!_cache.TryGetValue(scope, out var cacheEntry))
        {
            return false;
        }

        if (cacheEntry.IsExpired)
        {
            _cache.TryRemove(scope, out _);
            return false;
        }

        entries = cacheEntry.Entries;
        return true;
    }

    /// <summary>
    /// Caches all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The scope key.</param>
    /// <param name="entries">The configuration entries to cache.</param>
    /// <param name="cacheDuration">The duration the cache should be valid for. If null or zero, cache is not stored.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> is null.</exception>
    public void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries, TimeSpan? cacheDuration = null)
    {
        scope ??= string.Empty;

        ArgumentNullException.ThrowIfNull(entries);

        if (cacheDuration == null || cacheDuration == TimeSpan.Zero)
        {
            return;
        }

        var cacheEntry = new CacheEntry
        {
            Entries = entries,
            ExpiresAt = DateTime.UtcNow.Add(cacheDuration.Value)
        };

        _cache[scope] = cacheEntry;
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
