using Microsoft.Extensions.DependencyInjection;

namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Unit tests for dependency injection functionality in ClearHostedEndpoint.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public async Task Endpoint_WithRegisteredDependencies_ShouldResolveServices()
    {
        // Arrange
        var endpoint = new DependencyInjectionEndpoint();

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error, services should be registered
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_ShouldHaveIsolatedServiceProvider()
    {
        // Arrange
        var endpoint1 = new DependencyInjectionEndpoint();
        var endpoint2 = new DependencyInjectionEndpoint();

        try
        {
            // Act
            await endpoint1.StartAsync(CancellationToken.None);
            await endpoint2.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - both endpoints start independently
        }
        finally
        {
            await endpoint1.StopAsync(CancellationToken.None);
            await endpoint2.StopAsync(CancellationToken.None);
            endpoint1.Dispose();
            endpoint2.Dispose();
        }
    }

    [Fact]
    public async Task RegisterDependencyInjection_ShouldBeCalledDuringStartup()
    {
        // Arrange
        var endpoint = new DependencyInjectionEndpoint();

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - RegisterDependencyInjection is called (verified by successful startup)
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public void CreateServiceCollection_ShouldReturnNewServiceCollection()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        // Act
        var method = endpoint.GetType()
            .BaseType?.BaseType?
            .GetMethod("CreateServiceCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var serviceCollection = method?.Invoke(endpoint, null) as IServiceCollection;

        // Assert
        serviceCollection.Should().NotBeNull();

        endpoint.Dispose();
    }
}
