using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace ConfigR.Core;

/// <summary>
/// Default implementation of the ConfigR configuration manager.
/// </summary>
public sealed class DefaultConfigR : IConfigR
{
    private readonly IConfigStore _store;
    private readonly IConfigCache _cache;
    private readonly IConfigSerializer _serializer;
    private readonly IConfigKeyFormatter _keyFormatter;
    private readonly IOptions<ConfigROptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultConfigR"/> class.
    /// </summary>
    /// <param name="store">The configuration store.</param>
    /// <param name="cache">The configuration cache.</param>
    /// <param name="serializer">The configuration serializer.</param>
    /// <param name="keyFormatter">The configuration key formatter.</param>
    /// <param name="options">The ConfigR options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
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

    /// <summary>
    /// Gets a strongly-typed configuration instance synchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <returns>A new instance of the configuration type with populated values.</returns>
    public T Get<T>() where T : new()
        => GetAsync<T>().GetAwaiter().GetResult();

    /// <summary>
    /// Gets a strongly-typed configuration instance asynchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <returns>A task that returns a new instance of the configuration type with populated values.</returns>
    public async Task<T> GetAsync<T>() where T : new()
    {
        var type = typeof(T);
        var scope = GetScopeOrNull();
        var scopeKey = GetScopeKey(scope);
        var cacheDuration = _options.Value.CacheDuration;

        if (!_cache.TryGetAll(scopeKey, out var entries, cacheDuration))
        {
            var loaded = await _store.GetAllAsync(scope).ConfigureAwait(false)
                         ?? new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

            entries = loaded;
            _cache.SetAll(scopeKey, entries, cacheDuration);
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

    /// <summary>
    /// Saves a strongly-typed configuration instance synchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <param name="config">The configuration instance to save.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public void Save<T>(T config)
        => SaveAsync(config).GetAwaiter().GetResult();

    /// <summary>
    /// Saves a strongly-typed configuration instance asynchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <param name="config">The configuration instance to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
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

    /// <summary>
    /// Gets the properties of a configuration type that can be read and written.
    /// </summary>
    /// <param name="type">The configuration type.</param>
    /// <returns>An enumerable of public properties that can be read and written.</returns>
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

    /// <summary>
    /// Gets the effective scope, returning null if the default scope is empty or whitespace.
    /// </summary>
    /// <returns>The scope or null if no scope is configured.</returns>
    private string? GetScopeOrNull()
    {
        var scope = _options.Value.DefaultScope is null ? null : _options.Value.DefaultScope();
        return string.IsNullOrWhiteSpace(scope) ? null : scope;
    }

    /// <summary>
    /// Gets the cache key for a given scope.
    /// </summary>
    /// <param name="scope">The scope or null.</param>
    /// <returns>The cache key string.</returns>
    private static string GetScopeKey(string? scope)
        => scope ?? string.Empty;
}
