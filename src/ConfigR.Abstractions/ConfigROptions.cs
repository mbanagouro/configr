namespace ConfigR.Abstractions;

/// <summary>
/// Configuration options for ConfigR.
/// </summary>
public sealed class ConfigROptions
{
    /// <summary>
    /// Gets or sets the default scope for configuration entries.
    /// </summary>
    public Func<string> DefaultScope { get; set; } = () => "Default";

    /// <summary>
    /// Gets or sets the cache duration for configuration entries.
    /// If set to null or TimeSpan.Zero, caching is disabled.
    /// </summary>
    public TimeSpan? CacheDuration { get; set; } = TimeSpan.FromMinutes(10);
}
