namespace ConfigR.Npgsql;

/// <summary>
/// Configuration options for the PostgreSQL configuration store.
/// </summary>
public sealed class NpgsqlConfigStoreOptions
{
    /// <summary>
    /// Gets or sets the PostgreSQL connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the database schema name.
    /// Defaults to "public".
    /// </summary>
    public string Schema { get; set; } = "public";

    /// <summary>
    /// Gets or sets the table name where configuration entries are stored.
    /// Defaults to "configr".
    /// </summary>
    public string Table { get; set; } = "configr";

    /// <summary>
    /// Gets or sets a value indicating whether to automatically create the configuration table if it does not exist.
    /// </summary>
    public bool AutoCreateTable { get; set; }
}
