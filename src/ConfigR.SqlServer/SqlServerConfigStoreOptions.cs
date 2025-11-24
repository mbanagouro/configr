namespace ConfigR.SqlServer;

public sealed class SqlServerConfigStoreOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public string Table { get; set; } = "Configuracoes";
    public bool AutoCreateTable { get; set; }
}
