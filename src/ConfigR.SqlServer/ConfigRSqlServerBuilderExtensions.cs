using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConfigR.SqlServer;

/// <summary>
/// Extension methods for configuring SQL Server as the configuration store.
/// </summary>
public static class ConfigRSqlServerBuilderExtensions
{
    /// <summary>
    /// Configures ConfigR to use SQL Server as the configuration store.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="configure">Optional action to configure SQL Server options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null, empty, or whitespace.</exception>
    public static IConfigRBuilder UseSqlServer(
        this IConfigRBuilder builder,
        string connectionString,
        Action<SqlServerConfigStoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string must be provided.", nameof(connectionString));
        }

        builder.Services.AddOptions<SqlServerConfigStoreOptions>()
            .Configure(options =>
            {
                options.ConnectionString = connectionString;
                configure?.Invoke(options);
            });

        builder.Services.TryAddSingleton<IConfigStore, SqlServerConfigStore>();

        return builder;
    }
}
