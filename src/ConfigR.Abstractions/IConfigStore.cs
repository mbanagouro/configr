namespace ConfigR.Abstractions;

public interface IConfigStore
{
    Task<ConfigEntry?> GetAsync(string key, string? scope = null);
    Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null);
    Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null);
}
