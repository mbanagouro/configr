namespace ConfigR.Abstractions;

/// <summary>
/// Represents a configuration entry with a key, value, and optional scope.
/// </summary>
public sealed class ConfigEntry
{
    /// <summary>
    /// Gets or sets the configuration key.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration value.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional scope associated with this configuration entry.
    /// </summary>
    public string? Scope { get; init; }
}
