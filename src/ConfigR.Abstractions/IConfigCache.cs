namespace ConfigR.Abstractions;

public interface IConfigCache
{
    bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries);
    void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries);
    void Clear(string scope);
    void ClearAll();
}
