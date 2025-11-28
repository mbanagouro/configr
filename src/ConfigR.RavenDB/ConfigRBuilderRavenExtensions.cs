using System.Linq;
using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;

namespace ConfigR.RavenDB;

/// <summary>
/// Extension methods for configuring RavenDB as the configuration store.
/// </summary>
public static class ConfigRBuilderRavenExtensions
{
    /// <summary>
    /// Configures ConfigR to use RavenDB as the configuration store with a custom configuration action.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="configure">Action to configure RavenDB options.</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
    public static IConfigRBuilder UseRavenDb(
        this IConfigRBuilder builder,
        Action<RavenDbConfigStoreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.AddOptions<RavenDbConfigStoreOptions>()
            .Configure(configure);

        builder.Services.AddSingleton(CreateDocumentStore);
        builder.Services.AddSingleton<IConfigStore, RavenDbConfigStore>();

        return builder;
    }

    /// <summary>
    /// Configures ConfigR to use RavenDB as the configuration store.
    /// </summary>
    /// <param name="builder">The ConfigR builder.</param>
    /// <param name="urls">The RavenDB server URLs.</param>
    /// <param name="database">The database name.</param>
    /// <param name="keyPrefix">The prefix used for document identifiers. Defaults to "configr".</param>
    /// <returns>The <see cref="IConfigRBuilder"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when URLs or database are not provided.</exception>
    public static IConfigRBuilder UseRavenDb(
        this IConfigRBuilder builder,
        IEnumerable<string> urls,
        string database,
        string keyPrefix = "configr")
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (urls is null || !urls.Any())
        {
            throw new ArgumentException("At least one RavenDB URL must be provided.", nameof(urls));
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentException("Database must be provided.", nameof(database));
        }

        builder.Services.AddOptions<RavenDbConfigStoreOptions>()
            .Configure(options =>
            {
                options.Urls = urls.ToArray();
                options.Database = database;
                options.KeyPrefix = keyPrefix;
            });

        builder.Services.AddSingleton(CreateDocumentStore);
        builder.Services.AddSingleton<IConfigStore, RavenDbConfigStore>();

        return builder;
    }

    private static IDocumentStore CreateDocumentStore(IServiceProvider provider)
    {
        var options = provider
            .GetRequiredService<IOptions<RavenDbConfigStoreOptions>>()
            .Value;

        var store = new DocumentStore
        {
            Urls = options.Urls,
            Database = options.Database
        };

        store.Initialize();
        return store;
    }
}
