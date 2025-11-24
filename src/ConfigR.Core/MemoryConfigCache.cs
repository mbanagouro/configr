using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ConfigR.Abstractions;

namespace ConfigR.Core;

public sealed class MemoryConfigCache : IConfigCache
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, ConfigEntry>> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries)
    {
        scope ??= string.Empty;
        return _cache.TryGetValue(scope, out entries!);
    }

    public void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries)
    {
        scope ??= string.Empty;

        if (entries is null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        _cache[scope] = entries;
    }

    public void Clear(string scope)
    {
        scope ??= string.Empty;
        _cache.TryRemove(scope, out _);
    }

    public void ClearAll()
    {
        _cache.Clear();
    }
}
