using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;
using System.Text.RegularExpressions;

namespace ConfigR.MySql;

/// <summary>
/// MySQL implementation of the configuration store.
/// </summary>
public sealed class MySqlConfigStore : IConfigStore
{
    private readonly MySqlConfigStoreOptions _options;
    
    // Regex para validar identificadores SQL seguros
    private static readonly Regex SafeSqlIdentifierRegex = new(@"^[a-zA-Z_][a-zA-Z0-9_]{0,64}$", RegexOptions.Compiled);
    
    // Tamanho máximo para valores de configuração (100MB)
    private const int MaxValueSize = 100 * 1024 * 1024;
    private const int MaxKeySize = 255;
    private const int MaxScopeSize = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlConfigStore"/> class.
    /// </summary>
    /// <param name="options">The MySQL configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="options"/>.Value is null.</exception>
    public MySqlConfigStore(IOptions<MySqlConfigStoreOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        
        // Validar nome da tabela para prevenir SQL injection
        ValidateSqlIdentifier(_options.Table, nameof(_options.Table));
        
        EnsureTable();
    }

    /// <summary>
    /// Gets the configured table name.
    /// </summary>
    private string Table => _options.Table;
    
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
                $"Invalid SQL identifier '{identifier}'. Only alphanumeric characters and underscores are allowed, must start with a letter or underscore, and be max 64 characters.",
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
    /// Valida tamanhos de key e scope.
    /// </summary>
    private static void ValidateInputSizes(string key, string? scope)
    {
        if (key.Length > MaxKeySize)
        {
            throw new ArgumentException(
                $"Configuration key '{key}' exceeds maximum allowed length of {MaxKeySize} characters.",
                nameof(key));
        }
        
        if (scope?.Length > MaxScopeSize)
        {
            throw new ArgumentException(
                $"Scope '{scope}' exceeds maximum allowed length of {MaxScopeSize} characters.",
                nameof(scope));
        }
    }

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
        ValidateInputSizes(key, scope);
        
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
        if (result is null)
        {
            return null;
        }

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
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }
            
            var effectiveScope = scope ?? entry.Scope;
            
            // Validar tamanhos
            ValidateInputSizes(entry.Key, effectiveScope);
            ValidateValueSize(entry.Value, entry.Key);
            
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@key", entry.Key);
            cmd.Parameters.AddWithValue("@value", entry.Value ?? string.Empty);
            cmd.Parameters.AddWithValue("@scope", DbNullIfNull(effectiveScope));
            
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
