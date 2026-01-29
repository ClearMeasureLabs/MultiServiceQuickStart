using Microsoft.Extensions.DependencyInjection;

namespace ClearMeasure.HostedService.Interfaces;

/// <summary>
/// Defines the contract for dependency registration.
/// </summary>
public interface IServiceRegistration
{
    /// <summary>
    /// Registers dependencies into the service collection.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    void RegisterDependencies(IServiceCollection services);
}
