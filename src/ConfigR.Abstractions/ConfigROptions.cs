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
}
