using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConfigR.SqlServer;

public static class ConfigRSqlServerBuilderExtensions
{
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
