using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigR.Core;

public sealed class ConfigRBuilder : IConfigRBuilder
{
    public IServiceCollection Services { get; }

    public ConfigRBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
