namespace ConfigR.MySql;

/// <summary>
/// Configuration options for the MySQL configuration store.
/// </summary>
public sealed class MySqlConfigStoreOptions
{
    /// <summary>
    /// Gets or sets the MySQL connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the table name where configuration entries are stored.
    /// Defaults to "configr".
    /// </summary>
    public string Table { get; set; } = "configr";
}
