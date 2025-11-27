using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ConfigR.Redis;

/// <summary>
/// Redis implementation of the configuration store.
/// </summary>
public sealed class RedisConfigStore : IConfigStore
{
    private readonly RedisConfigStoreOptions _options;
    private readonly IConnectionMultiplexer _redis;

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
    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        var db = _redis.GetDatabase();

        foreach (var entry in entries)
        {
            await db.StringSetAsync(FormatKey(entry.Key, scope), entry.Value);
        }
    }
}