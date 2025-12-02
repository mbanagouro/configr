using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Text.RegularExpressions;

namespace ConfigR.Npgsql;

/// <summary>
/// PostgreSQL implementation of the configuration store using Npgsql.
/// </summary>
public sealed class NpgsqlConfigStore : IConfigStore
{
    private readonly NpgsqlConfigStoreOptions _options;
    
    // Regex para validar identificadores SQL seguros (PostgreSQL permite até 63 caracteres)
    private static readonly Regex SafeSqlIdentifierRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]{0,62}$", RegexOptions.Compiled);
    
    // Tamanho máximo para valores de configuração (100MB)
    private const int MaxValueSize = 100 * 1024 * 1024;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlConfigStore"/> class.
    /// </summary>
    /// <param name="options">The PostgreSQL configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="options"/>.Value is null.</exception>
    public NpgsqlConfigStore(IOptions<NpgsqlConfigStoreOptions> options)
    {
        _options = options.Value
            ?? throw new ArgumentNullException(nameof(options));
        
        // Validar schema e table para prevenir SQL injection
        ValidateSqlIdentifier(_options.Schema, nameof(_options.Schema));
        ValidateSqlIdentifier(_options.Table, nameof(_options.Table));

        if (_options.AutoCreateTable)
            EnsureTable();
    }
    
    /// <summary>
    /// Valida que um identificador SQL é seguro e não contém caracteres maliciosos.
    /// </summary>
    private static void ValidateSqlIdentifier(string identifier, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("SQL identifier cannot be null or empty.", parameterName);
        }
        
        if (!SafeSqlIdentifierRegex.IsMatch(identifier))
        {
            throw new ArgumentException(
                $"Invalid SQL identifier '{identifier}'. Only alphanumeric characters and underscores are allowed, must start with a letter or underscore, and be max 63 characters.",
                parameterName);
        }
    }
    
    /// <summary>
    /// Valida o tamanho do valor para prevenir ataques de negação de serviço.
    /// </summary>
    private static void ValidateValueSize(string? value, string key)
    {
        if (value?.Length > MaxValueSize)
        {
            throw new ArgumentException(
                $"Configuration value for key '{key}' exceeds maximum allowed size of {MaxValueSize} bytes.",
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that the configuration table exists in the database, creating it if necessary.
    /// </summary>
    private void EnsureTable()
    {
        using var conn = new NpgsqlConnection(_options.ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            CREATE SCHEMA IF NOT EXISTS {_options.Schema};

            CREATE TABLE IF NOT EXISTS {_options.Schema}.{_options.Table} (
                id SERIAL PRIMARY KEY,
                key TEXT NOT NULL,
                value TEXT NOT NULL,
                scope TEXT NULL,
                UNIQUE(key, scope)
            );";

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Gets the fully qualified table name in the format schema.table.
    /// </summary>
    private string FullTable => $"{_options.Schema}.{_options.Table}";

    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT value FROM {FullTable}
            WHERE key = @key AND (scope = @scope OR (@scope IS NULL AND scope IS NULL));
            ";

        cmd.Parameters.AddWithValue("key", key);
        cmd.Parameters.AddWithValue("scope", (object?)scope ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
            return null;

        return new ConfigEntry
        {
            Key = key,
            Value = result as string,
            Scope = scope
        };
    }

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A read-only dictionary of configuration entries.</returns>
    public async Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = scope is null
            ? $"SELECT key, value FROM {FullTable} WHERE scope IS NULL"
            : $"SELECT key, value FROM {FullTable} WHERE scope = @scope";

        if (scope != null)
            cmd.Parameters.AddWithValue("scope", scope);

        var result = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var key = reader.GetString(0);
            var value = reader.GetString(1);
            result[key] = new ConfigEntry
            {
                Key = key,
                Value = value,
                Scope = scope
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
    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        ArgumentNullException.ThrowIfNull(entries); 

        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var transaction = await conn.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            await using var cmd = conn.CreateCommand();
            {
                cmd.Transaction = transaction;
                cmd.CommandText = $@"
                    INSERT INTO {FullTable} (key, value, scope)
                    VALUES (@key, @value, @scope)
                    ON CONFLICT (key, scope)
                    DO UPDATE SET value = EXCLUDED.value;
                    ";

                foreach (var entry in entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key))
                    {
                        continue;
                    }
                    
                    var effectiveScope = scope ?? entry.Scope;
                    
                    // Validar tamanho do valor
                    ValidateValueSize(entry.Value, entry.Key);
                    
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("key", entry.Key);
                    cmd.Parameters.AddWithValue("value", entry.Value ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("scope", (object?)effectiveScope ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
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
}
