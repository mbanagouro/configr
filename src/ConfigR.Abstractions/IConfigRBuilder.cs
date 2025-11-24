using Microsoft.Extensions.DependencyInjection;

namespace ConfigR.Abstractions;

/// <summary>
/// Defines the contract for the ConfigR builder used to configure and extend ConfigR functionality.
/// </summary>
public interface IConfigRBuilder
{
    /// <summary>
    /// Gets the service collection used to register ConfigR services.
    /// </summary>
    IServiceCollection Services { get; }
}
