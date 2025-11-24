using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConfigR.MongoDB;

/// <summary>
/// Extension methods for configuring MongoDB as the configuration store.
/// </summary>
public static class ConfigRBuilderMongoExtensions
{
    /// <summary>
    /// Configures ConfigR to use MongoDB as the configuration store.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <param name="database">The MongoDB database name.</param>
    /// <param name="configure">Optional action to configure MongoDB options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IConfigRBuilder UseMongoDb(
        this IConfigRBuilder builder,
        string connectionString,
        string database,
        Action<MongoConfigStoreOptions>? configure = null)
    {
        builder.Services.AddOptions<MongoConfigStoreOptions>()
            .Configure(options =>
            {
                options.ConnectionString = connectionString;
                options.Database = database;
                configure?.Invoke(options);
            });

        builder.Services.AddSingleton<IConfigStore, MongoConfigStore>();

        return builder;
    }

    /// <summary>
    /// Configures ConfigR to use MongoDB as the configuration store with a custom configuration action.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="configure">Action to configure MongoDB options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
    public static IConfigRBuilder UseMongoDb(
        this IConfigRBuilder builder,
        Action<MongoConfigStoreOptions> configure)
    {
        builder.Services.AddOptions<MongoConfigStoreOptions>()
            .Configure(configure);

        builder.Services.AddSingleton<IConfigStore, MongoConfigStore>();

        return builder;
    }
}
