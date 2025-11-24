using ConfigR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigR.Core;

/// <summary>
/// Default implementation of the ConfigR builder.
/// </summary>
public sealed class ConfigRBuilder : IConfigRBuilder
{
    /// <summary>
    /// Gets the service collection used to register ConfigR services.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigRBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public ConfigRBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
