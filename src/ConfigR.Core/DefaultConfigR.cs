using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace ConfigR.Core;

public sealed class DefaultConfigR : IConfigR
{
    private readonly IConfigStore _store;
    private readonly IConfigCache _cache;
    private readonly IConfigSerializer _serializer;
    private readonly IConfigKeyFormatter _keyFormatter;
    private readonly IOptions<ConfigROptions> _options;

    public DefaultConfigR(
        IConfigStore store,
        IConfigCache cache,
        IConfigSerializer serializer,
        IConfigKeyFormatter keyFormatter,
        IOptions<ConfigROptions> options)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _keyFormatter = keyFormatter ?? throw new ArgumentNullException(nameof(keyFormatter));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public T Get<T>() where T : new()
        => GetAsync<T>().GetAwaiter().GetResult();

    public async Task<T> GetAsync<T>() where T : new()
    {
        var type = typeof(T);
        var scope = GetScopeOrNull();
        var scopeKey = GetScopeKey(scope);

        if (!_cache.TryGetAll(scopeKey, out var entries))
        {
            var loaded = await _store.GetAllAsync(scope).ConfigureAwait(false)
                         ?? new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

            entries = loaded;
            _cache.SetAll(scopeKey, entries);
        }

        var instance = new T();

        foreach (var prop in GetConfigProperties(type))
        {
            var key = _keyFormatter.GetKey(type, prop.Name);

            if (!entries.TryGetValue(key, out var entry))
            {
                continue;
            }

            var value = _serializer.Deserialize(entry.Value, prop.PropertyType);
            prop.SetValue(instance, value);
        }

        return instance;
    }

    public void Save<T>(T config)
        => SaveAsync(config).GetAwaiter().GetResult();

    public async Task SaveAsync<T>(T config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var type = typeof(T);
        var scope = GetScopeOrNull();
        var scopeKey = GetScopeKey(scope);

        var entries = new List<ConfigEntry>();

        foreach (var prop in GetConfigProperties(type))
        {
            var key = _keyFormatter.GetKey(type, prop.Name);
            var valueObj = prop.GetValue(config);
            var serialized = _serializer.Serialize(valueObj);

            entries.Add(new ConfigEntry
            {
                Key = key,
                Value = serialized,
                Scope = scope
            });
        }

        if (entries.Count == 0)
        {
            return;
        }

        await _store.UpsertAsync(entries, scope).ConfigureAwait(false);

        _cache.Clear(scopeKey);
    }

    private static IEnumerable<PropertyInfo> GetConfigProperties(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        foreach (var prop in type.GetProperties(flags))
        {
            if (!prop.CanRead || !prop.CanWrite)
            {
                continue;
            }

            if (prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            yield return prop;
        }
    }

    private string? GetScopeOrNull()
    {
        var scope = _options.Value.DefaultScope;
        return string.IsNullOrWhiteSpace(scope) ? null : scope;
    }

    private static string GetScopeKey(string? scope)
        => scope ?? string.Empty;
}
