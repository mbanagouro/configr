using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace ConfigR.Redis;

/// <summary>
/// Redis implementation of the configuration store.
/// </summary>
public sealed class RedisConfigStore : IConfigStore
{
    private readonly RedisConfigStoreOptions _options;
    private readonly IConnectionMultiplexer _redis;
    
    // Regex para validar prefixos seguros
    private static readonly Regex SafeKeyPrefixRegex = new(@"^[a-zA-Z0-9_\-:]{1,100}$", RegexOptions.Compiled);
    
    // Tamanho máximo para valores de configuração (100MB)
    private const int MaxValueSize = 100 * 1024 * 1024;
    private const int MaxKeySize = 256;
    private const int MaxScopeSize = 128;

    /// <summary>
    /// Default scope value used when no scope is provided.
    /// </summary>
    private const string DefaultScope = "default";

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConfigStore"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="options">The Redis configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="redis"/> or <paramref name="options"/> is null.</exception>
    public RedisConfigStore(
        IConnectionMultiplexer redis,
        IOptions<RedisConfigStoreOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        
        // Validar key prefix
        if (!string.IsNullOrWhiteSpace(_options.KeyPrefix))
        {
            ValidateKeyPrefix(_options.KeyPrefix);
        }
    }
    
    /// <summary>
    /// Valida que um prefixo de chave é seguro.
    /// </summary>
    private static void ValidateKeyPrefix(string prefix)
    {
        if (!SafeKeyPrefixRegex.IsMatch(prefix))
        {
            throw new ArgumentException(
                $"Invalid key prefix '{prefix}'. Only alphanumeric characters, underscores, hyphens, and colons are allowed (max 100 characters).",
                nameof(prefix));
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
    /// Formats a configuration key using the key prefix and scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The formatted Redis key.</returns>
    private string FormatKey(string key, string? scope)
    {
        scope ??= DefaultScope;
        return $"{_options.KeyPrefix}:{scope}:{key}";
    }

    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must be provided.", nameof(key));
        }
        
        ValidateInputSizes(key, scope);
        
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(FormatKey(key, scope));
        if (!value.HasValue)
        {
            return null;
        }

        return new ConfigEntry
        {
            Key = key,
            Value = value,
            Scope = scope ?? DefaultScope
        };
    }

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A read-only dictionary of configuration entries.</returns>
    public async Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        scope ??= DefaultScope;

        var prefix = $"{_options.KeyPrefix}:{scope}:";

        var keys = server.Keys(pattern: prefix + "*").ToArray();

        var result = new Dictionary<string, ConfigEntry>();

        foreach (var key in keys)
        {
            var value = await db.StringGetAsync(key);
            if (value.HasValue)
            {
                var keyName = key.ToString().Replace(prefix, "");
                result[keyName] = new ConfigEntry
                {
                    Key = keyName,
                    Value = value!,
                    Scope = scope
                };
            }
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
        
        var db = _redis.GetDatabase();

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
            
            await db.StringSetAsync(FormatKey(entry.Key, effectiveScope), entry.Value ?? string.Empty);
        }
    }
}