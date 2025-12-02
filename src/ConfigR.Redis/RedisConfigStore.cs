using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Buffers;
using System.Text.RegularExpressions;

namespace ConfigR.Redis;

/// <summary>
/// Redis implementation of the configuration store with low-allocation optimizations.
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
    /// Formats a configuration key using the key prefix and scope with minimal allocations.
    /// Uses Span<char> and ArrayPool for efficient string building.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The formatted Redis key.</returns>
    private string FormatKey(string key, string? scope)
    {
        var effectiveScope = scope ?? DefaultScope;
        var prefix = _options.KeyPrefix;
        
        // Calculate total length: prefix + ':' + scope + ':' + key
        var totalLength = prefix.Length + 1 + effectiveScope.Length + 1 + key.Length;
        
        // Use stack allocation for small keys
        Span<char> buffer = stackalloc char[256];
        
        if (totalLength <= buffer.Length)
        {
            // Fast path: use stack allocation
            return BuildKeyOnStack(buffer[..totalLength], prefix.AsSpan(), effectiveScope.AsSpan(), key.AsSpan());
        }
        
        // Slow path: use ArrayPool for larger keys
        var rentedArray = ArrayPool<char>.Shared.Rent(totalLength);
        try
        {
            var poolBuffer = rentedArray.AsSpan(0, totalLength);
            return BuildKeyOnStack(poolBuffer, prefix.AsSpan(), effectiveScope.AsSpan(), key.AsSpan());
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }
    
    /// <summary>
    /// Builds a Redis key on stack/pool buffer without intermediate allocations.
    /// </summary>
    private static string BuildKeyOnStack(
        Span<char> buffer,
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> scope,
        ReadOnlySpan<char> key)
    {
        var position = 0;
        
        // Copy prefix
        prefix.CopyTo(buffer[position..]);
        position += prefix.Length;
        
        // Add separator
        buffer[position++] = ':';
        
        // Copy scope
        scope.CopyTo(buffer[position..]);
        position += scope.Length;
        
        // Add separator
        buffer[position++] = ':';
        
        // Copy key
        key.CopyTo(buffer[position..]);
        
        return new string(buffer);
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
        var redisKey = FormatKey(key, scope);
        var value = await db.StringGetAsync(redisKey);
        
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

        var effectiveScope = scope ?? DefaultScope;
        
        // Build prefix using Span to avoid allocations
        var prefix = BuildScopePrefix(_options.KeyPrefix, effectiveScope);

        var keys = server.Keys(pattern: prefix + "*").ToArray();

        var result = new Dictionary<string, ConfigEntry>(keys.Length);

        foreach (var key in keys)
        {
            var value = await db.StringGetAsync(key);
            if (value.HasValue)
            {
                var keyString = key.ToString();
                var keyName = ExtractKeyFromRedisKey(keyString.AsSpan(), prefix.AsSpan());
                
                result[keyName] = new ConfigEntry
                {
                    Key = keyName,
                    Value = value!,
                    Scope = effectiveScope
                };
            }
        }

        return result;
    }
    
    /// <summary>
    /// Builds scope prefix for pattern matching.
    /// </summary>
    private static string BuildScopePrefix(string prefix, string scope)
    {
        var totalLength = prefix.Length + 1 + scope.Length + 1;
        
        Span<char> buffer = stackalloc char[128];
        
        if (totalLength <= buffer.Length)
        {
            var span = buffer[..totalLength];
            prefix.AsSpan().CopyTo(span);
            span[prefix.Length] = ':';
            scope.AsSpan().CopyTo(span[(prefix.Length + 1)..]);
            span[totalLength - 1] = ':';
            return new string(span);
        }
        
        // Fallback for very long prefixes
        return $"{prefix}:{scope}:";
    }
    
    /// <summary>
    /// Extracts the key name from a full Redis key without allocations.
    /// </summary>
    private static string ExtractKeyFromRedisKey(ReadOnlySpan<char> redisKey, ReadOnlySpan<char> prefix)
    {
        if (redisKey.StartsWith(prefix))
        {
            var keyPart = redisKey[prefix.Length..];
            return new string(keyPart);
        }
        
        return new string(redisKey);
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
            
            var redisKey = FormatKey(entry.Key, effectiveScope);
            await db.StringSetAsync(redisKey, entry.Value ?? string.Empty);
        }
    }
}