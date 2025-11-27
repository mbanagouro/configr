using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigR.MySql;

/// <summary>
/// Extension methods for configuring MySQL as the configuration store.
/// </summary>
public static class ConfigRBuilderMySqlExtensions
{
    /// <summary>
    /// Configures ConfigR to use MySQL as the configuration store with a custom configuration action.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="configure">Action to configure MySQL options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
    public static IConfigRBuilder UseMySql(
        this IConfigRBuilder builder,
        Action<MySqlConfigStoreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.Configure(configure);
        builder.Services.AddSingleton<IConfigStore, MySqlConfigStore>();
        return builder;
    }

    /// <summary>
    /// Configures ConfigR to use MySQL as the configuration store.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="connectionString">The MySQL connection string.</param>
    /// <param name="table">The table name for configuration entries. Defaults to "configr".</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IConfigRBuilder UseMySql(
        this IConfigRBuilder builder,
        string connectionString,
        string table = "configr")
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Configure<MySqlConfigStoreOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.Table = table;
        });

        builder.Services.AddSingleton<IConfigStore, MySqlConfigStore>();

        return builder;
    }
}
