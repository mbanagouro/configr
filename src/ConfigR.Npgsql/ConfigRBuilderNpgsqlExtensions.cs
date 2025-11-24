using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigR.Npgsql;

/// <summary>
/// Extension methods for configuring PostgreSQL as the configuration store.
/// </summary>
public static class ConfigRBuilderNpgsqlExtensions
{
    /// <summary>
    /// Configures ConfigR to use PostgreSQL as the configuration store with a custom configuration action.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="configure">Action to configure PostgreSQL options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
    public static IConfigRBuilder UseNpgsql(
        this IConfigRBuilder builder,
        Action<NpgsqlConfigStoreOptions> configure)
    {
        builder.Services.Configure(configure);
        builder.Services.AddSingleton<IConfigStore, NpgsqlConfigStore>();
        return builder;
    }

    /// <summary>
    /// Configures ConfigR to use PostgreSQL as the configuration store.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="schema">The database schema name. Defaults to "public".</param>
    /// <param name="table">The table name for configuration entries. Defaults to "configr".</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IConfigRBuilder UseNpgsql(
        this IConfigRBuilder builder,
        string connectionString,
        string schema = "public",
        string table = "configr")
    {
        builder.Services.Configure<NpgsqlConfigStoreOptions>(o =>
        {
            o.ConnectionString = connectionString;
            o.Schema = schema;
            o.Table = table;
        });

        builder.Services.AddSingleton<IConfigStore, NpgsqlConfigStore>();
        return builder;
    }
}
