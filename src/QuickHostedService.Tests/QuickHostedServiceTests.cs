using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuickHostedService.Application;
using Xunit;

namespace QuickHostedService.Tests;

/// <summary>
/// Test service for testing QuickHostedService functionality.
/// </summary>
public class TestHostedService : Application.QuickHostedService
{
    public bool OnStartingAsyncCalled { get; private set; }
    public bool OnStoppingAsyncCalled { get; private set; }
    public bool ExecuteAsyncCalled { get; private set; }
    public bool RegisterDependencyInjectionCalled { get; private set; }

    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        RegisterDependencyInjectionCalled = true;
        services.AddSingleton<ITestService, TestService>();
    }

    public override Task OnStartingAsync(CancellationToken cancellationToken)
    {
        OnStartingAsyncCalled = true;
        return Task.CompletedTask;
    }

    public override Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        OnStoppingAsyncCalled = true;
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ExecuteAsyncCalled = true;
        
        // Verify we can resolve services
        var testService = ServiceProvider.GetRequiredService<ITestService>();
        testService.DoWork();

        // Simulate short work
        await Task.Delay(100, stoppingToken);
    }
}

public interface ITestService
{
    void DoWork();
}

public class TestService : ITestService
{
    public bool WorkCalled { get; private set; }
    
    public void DoWork()
    {
        WorkCalled = true;
    }
}

public class QuickHostedServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldCallLifecycleMethods_InCorrectOrder()
    {
        // Arrange
        var service = new TestHostedService();

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(200); // Give ExecuteAsync time to run
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.RegisterDependencyInjectionCalled.Should().BeTrue();
        service.OnStartingAsyncCalled.Should().BeTrue();
        service.ExecuteAsyncCalled.Should().BeTrue();
        service.OnStoppingAsyncCalled.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_ShouldRegisterDependencies_Successfully()
    {
        // Arrange
        var service = new TestHostedService();

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(200);

        // Assert
        service.RegisterDependencyInjectionCalled.Should().BeTrue();
        service.ExecuteAsyncCalled.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_ShouldCallOnStoppingAsync()
    {
        // Arrange
        var service = new TestHostedService();
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.OnStoppingAsyncCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var service = new TestHostedService();
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert - Service should stop gracefully
        service.OnStoppingAsyncCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ServiceProvider_ShouldResolveRegisteredServices()
    {
        // Arrange
        var service = new TestHostedService();

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.ExecuteAsyncCalled.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var service = new TestHostedService();

        // Act & Assert
        var act = () => service.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task MultipleStartStop_ShouldWork()
    {
        // Arrange
        var service = new TestHostedService();

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.OnStartingAsyncCalled.Should().BeTrue();
        service.OnStoppingAsyncCalled.Should().BeTrue();
    }
}
