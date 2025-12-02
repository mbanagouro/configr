namespace ConfigR.Abstractions;

/// <summary>
/// Defines the contract for caching configuration entries.
/// </summary>
public interface IConfigCache
{
    /// <summary>
    /// Attempts to retrieve all cached configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The scope key.</param>
    /// <param name="entries">The cached configuration entries if found.</param>
    /// <returns>True if the entries were found in the cache and are not expired; otherwise, false.</returns>
    bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries);

    /// <summary>
    /// Caches all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The scope key.</param>
    /// <param name="entries">The configuration entries to cache.</param>
    /// <param name="cacheDuration">The duration the cache should be valid for. If null or zero, cache is not stored.</param>
    void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries, TimeSpan? cacheDuration = null);

    /// <summary>
    /// Clears the cache for a specific scope.
    /// </summary>
    /// <param name="scope">The scope key to clear.</param>
    void Clear(string scope);

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    void ClearAll();
}
