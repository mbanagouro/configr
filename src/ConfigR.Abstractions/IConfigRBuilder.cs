using Microsoft.Extensions.DependencyInjection;

namespace ConfigR.Abstractions;

public interface IConfigRBuilder
{
    IServiceCollection Services { get; }
}
