using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickHostedService.Application;

namespace QuickHostedService.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to simplify service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a hosted service to the service collection.
    /// </summary>
    /// <typeparam name="THostedService">The type of hosted service to add.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuickHostedService<THostedService>(this IServiceCollection services)
        where THostedService : Application.QuickHostedService
    {
        return services.AddHostedService<THostedService>();
    }

    /// <summary>
    /// Adds a hosted service to the service collection with a factory method.
    /// </summary>
    /// <typeparam name="THostedService">The type of hosted service to add.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="implementationFactory">The factory that creates the hosted service.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuickHostedService<THostedService>(
        this IServiceCollection services,
        Func<IServiceProvider, THostedService> implementationFactory)
        where THostedService : Application.QuickHostedService
    {
        return services.AddHostedService(implementationFactory);
    }
}
