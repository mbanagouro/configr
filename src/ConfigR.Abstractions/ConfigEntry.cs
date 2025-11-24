namespace ConfigR.Abstractions;

public sealed class ConfigEntry
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Scope { get; init; }
}
