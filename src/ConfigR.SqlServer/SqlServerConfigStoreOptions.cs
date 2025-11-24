namespace ConfigR.SqlServer;

/// <summary>
/// Configuration options for the SQL Server configuration store.
/// </summary>
public sealed class SqlServerConfigStoreOptions
{
    /// <summary>
    /// Gets or sets the SQL Server connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema name where the configuration table is located.
    /// Defaults to "dbo".
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the table name where configuration entries are stored.
    /// Defaults to "Configuracoes".
    /// </summary>
    public string Table { get; set; } = "ConfigR";

    /// <summary>
    /// Gets or sets a value indicating whether to automatically create the configuration table if it does not exist.
    /// </summary>
    public bool AutoCreateTable { get; set; }
}
