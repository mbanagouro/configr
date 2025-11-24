using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ConfigR.Core;

public static class ConfigRServiceCollectionExtensions
{
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
