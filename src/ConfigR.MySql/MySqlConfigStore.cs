using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;

namespace ConfigR.MySql;

/// <summary>
/// MySQL implementation of the configuration store.
/// </summary>
public sealed class MySqlConfigStore : IConfigStore
{
    private readonly MySqlConfigStoreOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlConfigStore"/> class.
    /// </summary>
    /// <param name="options">The MySQL configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="options"/>.Value is null.</exception>
    public MySqlConfigStore(IOptions<MySqlConfigStoreOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        EnsureTable();
    }

    /// <summary>
    /// Gets the configured table name.
    /// </summary>
    private string Table => _options.Table;

    /// <summary>
    /// Ensures that the configuration table exists in the database, creating it if necessary.
    /// </summary>
    private void EnsureTable()
    {
        using var conn = new MySqlConnection(_options.ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
        CREATE TABLE IF NOT EXISTS {Table} (
            id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
            cfg_key VARCHAR(255) NOT NULL,
            cfg_value TEXT NOT NULL,
            scope VARCHAR(255) NULL,
            UNIQUE KEY uk_config (cfg_key, scope)
        );";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Converts a string value to a database null value if the string is null.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The string value or DBNull.Value if the value is null.</returns>
    private static object? DbNullIfNull(string? value) =>
        value is null ? DBNull.Value : value;

    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        await using var conn = new MySqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT cfg_value FROM {Table}
            WHERE cfg_key = @key
              AND (scope = @scope OR (@scope IS NULL AND scope IS NULL));
            ";

        cmd.Parameters.AddWithValue("@key", key);
        cmd.Parameters.AddWithValue("@scope", DbNullIfNull(scope));

        var result = await cmd.ExecuteScalarAsync();
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
        await using var conn = new MySqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = scope is null
            ? $"SELECT cfg_key, cfg_value FROM {Table} WHERE scope IS NULL"
            : $"SELECT cfg_key, cfg_value FROM {Table} WHERE scope = @scope";

        if (scope != null)
            cmd.Parameters.AddWithValue("@scope", scope);

        var dict = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var key = reader.GetString(0);
            var value = reader.GetString(1);
            dict[key] = new ConfigEntry
            {
                Key = key,
                Value = value,
                Scope = scope
            };
        }

        return dict;
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

        await using var conn = new MySqlConnection(_options.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            INSERT INTO {Table} (cfg_key, cfg_value, scope)
            VALUES (@key, @value, @scope)
            ON DUPLICATE KEY UPDATE
                cfg_value = VALUES(cfg_value);
            ";

        foreach (var entry in entries)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@key", entry.Key);
            cmd.Parameters.AddWithValue("@value", entry.Value);
            cmd.Parameters.AddWithValue("@scope", DbNullIfNull(scope));
            
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
