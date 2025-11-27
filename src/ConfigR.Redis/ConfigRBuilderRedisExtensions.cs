using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ConfigR.Redis;

/// <summary>
/// Extension methods for configuring Redis as the configuration store.
/// </summary>
public static class ConfigRBuilderRedisExtensions
{
    /// <summary>
    /// Configures ConfigR to use Redis as the configuration store with a custom configuration action.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="configure">Action to configure Redis options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
    public static IConfigRBuilder UseRedis(
        this IConfigRBuilder builder,
        Action<RedisConfigStoreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.Configure(configure);

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<RedisConfigStoreOptions>>().Value;
            return ConnectionMultiplexer.Connect(opts.ConnectionString);
        });

        builder.Services.AddSingleton<IConfigStore, RedisConfigStore>();

        return builder;
    }

    /// <summary>
    /// Configures ConfigR to use Redis as the configuration store.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <param name="keyPrefix">The prefix for Redis keys. Defaults to "configr".</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IConfigRBuilder UseRedis(
        this IConfigRBuilder builder,
        string connectionString,
        string keyPrefix = "configr")
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Configure<RedisConfigStoreOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.KeyPrefix = keyPrefix;
        });

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(connectionString);
        });

        builder.Services.AddSingleton<IConfigStore, RedisConfigStore>();

        return builder;
    }
}