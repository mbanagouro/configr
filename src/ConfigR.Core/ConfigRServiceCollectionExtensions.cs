using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConfigR.Core;

/// <summary>
/// Extension methods for adding ConfigR to the dependency injection container.
/// </summary>
public static class ConfigRServiceCollectionExtensions
{
    /// <summary>
    /// Adds ConfigR services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Optional action to configure ConfigR options.</param>
    /// <returns>A <see cref="IConfigRBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IConfigRBuilder AddConfigR(
        this IServiceCollection services,
        Action<ConfigROptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<ConfigROptions>(_ => { });
        }

        services.TryAddSingleton<IConfigSerializer, DefaultConfigSerializer>();
        services.TryAddSingleton<IConfigKeyFormatter, DefaultConfigKeyFormatter>();
        services.TryAddSingleton<IConfigCache, MemoryConfigCache>();
        services.TryAddSingleton<IConfigR, DefaultConfigR>();

        return new ConfigRBuilder(services);
    }
}
