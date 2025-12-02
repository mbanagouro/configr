using ConfigR.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.RegularExpressions;

namespace ConfigR.SqlServer;

/// <summary>
/// SQL Server implementation of the configuration store.
/// </summary>
public sealed class SqlServerConfigStore : IConfigStore
{
    private readonly SqlServerConfigStoreOptions _options;
    private readonly string _fullTableName;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private bool _initialized;
    
    // Regex para validar identificadores SQL seguros (schema/table)
    private static readonly Regex SafeSqlIdentifierRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]{0,127}$", RegexOptions.Compiled);
    
    // Tamanho máximo para valores de configuração (100MB)
    private const int MaxValueSize = 100 * 1024 * 1024;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerConfigStore"/> class.
    /// </summary>
    /// <param name="options">The SQL Server configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null or <paramref name="options"/>.Value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection string is not configured.</exception>
    public SqlServerConfigStore(IOptions<SqlServerConfigStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("ConnectionString must be configured for SqlServerConfigStore.");
        }

        var schema = string.IsNullOrWhiteSpace(_options.Schema) ? "dbo" : _options.Schema;
        var table = string.IsNullOrWhiteSpace(_options.Table) ? "ConfigR" : _options.Table;
        
        // Validar schema e table para prevenir SQL injection
        ValidateSqlIdentifier(schema, nameof(_options.Schema));
        ValidateSqlIdentifier(table, nameof(_options.Table));

        _fullTableName = $"[{schema}].[{table}]";
    }
    
    /// <summary>
    /// Valida que um identificador SQL é seguro e não contém caracteres maliciosos.
    /// </summary>
    private static void ValidateSqlIdentifier(string identifier, string parameterName)
    {
        if (!SafeSqlIdentifierRegex.IsMatch(identifier))
        {
            throw new ArgumentException(
                $"Invalid SQL identifier '{identifier}'. Only alphanumeric characters and underscores are allowed, and it must start with a letter or underscore.",
                parameterName);
        }
    }
    
    /// <summary>
    /// Valida o tamanho do valor para prevenir ataques de negação de serviço.
    /// </summary>
    private static void ValidateValueSize(string value, string key)
    {
        if (value?.Length > MaxValueSize)
        {
            throw new ArgumentException(
                $"Configuration value for key '{key}' exceeds maximum allowed size of {MaxValueSize} bytes.",
                nameof(value));
        }
    }

    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or whitespace.</exception>
    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must be provided.", nameof(key));
        }

        await EnsureInitializedAsync().ConfigureAwait(false);

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $@"
            SELECT TOP (1) [Key], [Value], [Scope]
            FROM {_fullTableName}
            WHERE [Key] = @key
              AND ((@scope IS NULL AND [Scope] IS NULL) OR [Scope] = @scope);";

        command.Parameters.Add(new SqlParameter("@key", SqlDbType.NVarChar, 256) { Value = key });
        var scopeParam = new SqlParameter("@scope", SqlDbType.NVarChar, 128);
        scopeParam.Value = (object?)scope ?? DBNull.Value;
        command.Parameters.Add(scopeParam);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false);
        if (!await reader.ReadAsync().ConfigureAwait(false))
        {
            return null;
        }

        var resultKey = reader.GetString(reader.GetOrdinal("Key"));
        var value = reader.GetString(reader.GetOrdinal("Value"));
        var scopeValueOrdinal = reader.GetOrdinal("Scope");
        var scopeValue = await reader.IsDBNullAsync(scopeValueOrdinal).ConfigureAwait(false)
            ? null
            : reader.GetString(scopeValueOrdinal);

        return new ConfigEntry
        {
            Key = resultKey,
            Value = value,
            Scope = scopeValue
        };
    }

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A read-only dictionary of configuration entries.</returns>
    public async Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        var result = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $@"
            SELECT [Key], [Value], [Scope]
            FROM {_fullTableName}
            WHERE (@scope IS NULL AND [Scope] IS NULL) OR [Scope] = @scope;";

        var scopeParam = new SqlParameter("@scope", SqlDbType.NVarChar, 128);
        scopeParam.Value = (object?)scope ?? DBNull.Value;
        command.Parameters.Add(scopeParam);

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var key = reader.GetString(reader.GetOrdinal("Key"));
            var value = reader.GetString(reader.GetOrdinal("Value"));
            var scopeValueOrdinal = reader.GetOrdinal("Scope");
            var scopeValue = await reader.IsDBNullAsync(scopeValueOrdinal).ConfigureAwait(false)
                ? null
                : reader.GetString(scopeValueOrdinal);

            result[key] = new ConfigEntry
            {
                Key = key,
                Value = value,
                Scope = scopeValue
            };
        }

        return result;
    }

    /// <summary>
    /// Inserts or updates configuration entries.
    /// </summary>
    /// <param name="entries">The configuration entries to upsert.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A task representing the asynchronous upsert operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when entry validation fails.</exception>
    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        await EnsureInitializedAsync().ConfigureAwait(false);

        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = (SqlTransaction)transaction;
                command.CommandType = CommandType.Text;
                command.CommandText = $@"
                    IF EXISTS (
                        SELECT 1 FROM {_fullTableName}
                        WHERE [Key] = @key
                            AND ((@scope IS NULL AND [Scope] IS NULL) OR [Scope] = @scope)
                    )
                    BEGIN
                        UPDATE {_fullTableName}
                        SET [Value] = @value
                        WHERE [Key] = @key
                            AND ((@scope IS NULL AND [Scope] IS NULL) OR [Scope] = @scope);
                    END
                    ELSE
                    BEGIN
                        INSERT INTO {_fullTableName} ([Key], [Value], [Scope])
                        VALUES (@key, @value, @scope);
                    END";

                var keyParam = new SqlParameter("@key", SqlDbType.NVarChar, 256);
                var valueParam = new SqlParameter("@value", SqlDbType.NVarChar, -1);
                var scopeParam = new SqlParameter("@scope", SqlDbType.NVarChar, 128);

                command.Parameters.Add(keyParam);
                command.Parameters.Add(valueParam);
                command.Parameters.Add(scopeParam);

                foreach (var entry in entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key))
                    {
                        continue;
                    }
                    
                    // Validar tamanho da chave
                    if (entry.Key.Length > 256)
                    {
                        throw new ArgumentException(
                            $"Configuration key '{entry.Key}' exceeds maximum allowed length of 256 characters.",
                            nameof(entries));
                    }

                    var effectiveScope = scope ?? entry.Scope;
                    
                    // Validar tamanho do scope
                    if (effectiveScope?.Length > 128)
                    {
                        throw new ArgumentException(
                            $"Scope '{effectiveScope}' exceeds maximum allowed length of 128 characters.",
                            nameof(scope));
                    }
                    
                    // Validar tamanho do valor
                    if (entry.Value != null)
                    {
                        ValidateValueSize(entry.Value, entry.Key);
                    }

                    keyParam.Value = entry.Key;
                    valueParam.Value = (object?)entry.Value ?? string.Empty;
                    scopeParam.Value = (object?)effectiveScope ?? DBNull.Value;

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Ensures the store is initialized, creating the table if AutoCreateTable is enabled.
    /// </summary>
    /// <returns>A task representing the initialization operation.</returns>
    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            if (_options.AutoCreateTable)
            {
                await CreateTableIfNotExistsAsync().ConfigureAwait(false);
            }

            _initialized = true;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    /// <summary>
    /// Creates the configuration table in the database if it does not already exist.
    /// </summary>
    /// <returns>A task representing the table creation operation.</returns>
    private async Task CreateTableIfNotExistsAsync()
    {
        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var schema = string.IsNullOrWhiteSpace(_options.Schema) ? "dbo" : _options.Schema;
        var table = string.IsNullOrWhiteSpace(_options.Table) ? "ConfigR" : _options.Table;

        var commandText = $@"
            IF NOT EXISTS (
                SELECT 1
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE t.name = @tableName AND s.name = @schemaName
            )
            BEGIN
                EXEC('
                    CREATE TABLE [{schema}].[{table}] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Key] NVARCHAR(256) NOT NULL,
                        [Value] NVARCHAR(MAX) NOT NULL,
                        [Scope] NVARCHAR(128) NULL
                    );

                    CREATE UNIQUE INDEX IX_{table}_Key_Scope
                    ON [{schema}].[{table}] ([Key], [Scope]);
                ');
            END
            ";

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = commandText;

        command.Parameters.Add(new SqlParameter("@tableName", SqlDbType.NVarChar, 128) { Value = table });
        command.Parameters.Add(new SqlParameter("@schemaName", SqlDbType.NVarChar, 128) { Value = schema });

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }
}
