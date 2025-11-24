namespace ConfigR.Abstractions;

/// <summary>
/// Defines the contract for a configuration storage provider.
/// </summary>
public interface IConfigStore
{
    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    Task<ConfigEntry?> GetAsync(string key, string? scope = null);

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A read-only dictionary of configuration entries.</returns>
    Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null);

    /// <summary>
    /// Inserts or updates configuration entries.
    /// </summary>
    /// <param name="entries">The configuration entries to upsert.</param>
    /// <param name="scope">The optional scope.</param>
    Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null);
}
