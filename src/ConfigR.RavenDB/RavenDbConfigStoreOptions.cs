namespace ConfigR.RavenDB;

/// <summary>
/// Configuration options for the RavenDB configuration store.
/// </summary>
public sealed class RavenDbConfigStoreOptions
{
    /// <summary>
    /// Gets or sets the RavenDB server URLs.
    /// </summary>
    public required string[] Urls { get; set; }

    /// <summary>
    /// Gets or sets the RavenDB database name.
    /// </summary>
    public required string Database { get; set; }

    /// <summary>
    /// Gets or sets the prefix used for document identifiers.
    /// Defaults to "configr".
    /// </summary>
    public string KeyPrefix { get; set; } = "configr";
}
