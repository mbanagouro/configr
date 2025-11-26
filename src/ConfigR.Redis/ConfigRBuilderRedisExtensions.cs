using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ConfigR.Redis;

public static class ConfigRBuilderRedisExtensions
{
    public static IConfigRBuilder UseRedis(
        this IConfigRBuilder builder,
        Action<RedisConfigStoreOptions> configure)
    {
        builder.Services.Configure(configure);

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<RedisConfigStoreOptions>>().Value;
            return ConnectionMultiplexer.Connect(opts.ConnectionString);
        });

        builder.Services.AddSingleton<IConfigStore, RedisConfigStore>();

        return builder;
    }

    public static IConfigRBuilder UseRedis(
        this IConfigRBuilder builder,
        string connectionString,
        string keyPrefix = "configr")
    {
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