using ConfigR.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace ConfigR.SqlServer;

public sealed class SqlServerConfigStore : IConfigStore
{
    private readonly SqlServerConfigStoreOptions _options;
    private readonly string _fullTableName;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private bool _initialized;

    public SqlServerConfigStore(IOptions<SqlServerConfigStoreOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("ConnectionString must be configured for SqlServerConfigStore.");
        }

        var schema = string.IsNullOrWhiteSpace(_options.Schema) ? "dbo" : _options.Schema;
        var table = string.IsNullOrWhiteSpace(_options.Table) ? "Configuracoes" : _options.Table;

        _fullTableName = $"[{schema}].[{table}]";
    }

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

                    var effectiveScope = scope ?? entry.Scope;

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

    private async Task CreateTableIfNotExistsAsync()
    {
        await using var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var schema = string.IsNullOrWhiteSpace(_options.Schema) ? "dbo" : _options.Schema;
        var table = string.IsNullOrWhiteSpace(_options.Table) ? "Configuracoes" : _options.Table;

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
