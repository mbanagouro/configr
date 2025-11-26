using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ConfigR.Redis;

public sealed class RedisConfigStore : IConfigStore
{
    private readonly RedisConfigStoreOptions _options;
    private readonly IConnectionMultiplexer _redis;

    private const string DefaultScope = "default";

    public RedisConfigStore(
        IConnectionMultiplexer redis,
        IOptions<RedisConfigStoreOptions> options)
    {
        _options = options.Value;
        _redis = redis;
    }

    private string FormatKey(string key, string? scope)
    {
        scope ??= DefaultScope;
        return $"{_options.KeyPrefix}:{scope}:{key}";
    }

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

    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        var db = _redis.GetDatabase();

        foreach (var entry in entries)
        {
            await db.StringSetAsync(FormatKey(entry.Key, scope), entry.Value);
        }
    }
}