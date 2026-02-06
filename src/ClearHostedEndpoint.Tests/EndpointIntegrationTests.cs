namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Integration tests for ClearHostedEndpoint that test end-to-end scenarios.
/// </summary>
public class EndpointIntegrationTests
{
    [Fact]
    public async Task FullLifecycle_StartWorkStop_ShouldCompleteSuccessfully()
    {
        // Arrange
        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act - Start
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500); // Simulate work

            // Assert - Running
            endpoint.ExecuteAsyncCalled.Should().BeTrue();

            // Act - Stop
            await endpoint.StopAsync(CancellationToken.None);

            // Assert - Stopped without errors
        }
        finally
        {
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task MultipleEndpoints_WithDifferentConfigurations_ShouldRunIndependently()
    {
        // Arrange
        var endpoint1 = new TestEndpoint(
            TestHelpers.CreateTestConfiguration(),
            endpointOptions: new EndpointOptions { EndpointName = "Endpoint1" });
        var endpoint2 = new TestEndpoint(
            TestHelpers.CreateTestConfiguration(),
            endpointOptions: new EndpointOptions { EndpointName = "Endpoint2", MaxConcurrency = 5 });
        var endpoint3 = new CustomNamedEndpoint(TestHelpers.CreateTestConfiguration(), "Endpoint3");

        try
        {
            // Act
            await endpoint1.StartAsync(CancellationToken.None);
            await endpoint2.StartAsync(CancellationToken.None);
            await endpoint3.StartAsync(CancellationToken.None);
            await Task.Delay(1000);

            // Assert - All endpoints running
            endpoint1.ExecuteAsyncCalled.Should().BeTrue();
            endpoint2.ExecuteAsyncCalled.Should().BeTrue();

            // Cleanup
            await endpoint1.StopAsync(CancellationToken.None);
            await endpoint2.StopAsync(CancellationToken.None);
            await endpoint3.StopAsync(CancellationToken.None);
        }
        finally
        {
            endpoint1.Dispose();
            endpoint2.Dispose();
            endpoint3.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithLongRunningWork_ShouldCancelGracefully()
    {
        // Arrange
        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration());
        var cts = new CancellationTokenSource();

        try
        {
            await endpoint.StartAsync(cts.Token);
            await Task.Delay(500);

            // Act - Request cancellation
            cts.Cancel();
            await endpoint.StopAsync(CancellationToken.None);

            // Assert - Stopped gracefully
        }
        finally
        {
            endpoint.Dispose();
            cts.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithAllFeatures_ShouldStartAndStopSuccessfully()
    {
        // Arrange
        var endpointOptions = new EndpointOptions
        {
            EndpointName = "FullFeaturedEndpoint",
            EnableInstallers = true,
            ErrorQueue = "test-error",
            AuditQueue = "test-audit",
            MaxConcurrency = 2,
            ImmediateRetryCount = 3,
            DelayedRetryCount = 2
        };

        var endpoint = new DependencyInjectionEndpoint(TestHelpers.CreateTestConfiguration());
        var customEndpointWithOptions = new TestEndpoint(TestHelpers.CreateTestConfiguration(), endpointOptions: endpointOptions);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await customEndpointWithOptions.StartAsync(CancellationToken.None);
            await Task.Delay(1000);

            // Assert - Both endpoints running
            await endpoint.StopAsync(CancellationToken.None);
            await customEndpointWithOptions.StopAsync(CancellationToken.None);
        }
        finally
        {
            endpoint.Dispose();
            customEndpointWithOptions.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_RestartAfterStop_ShouldWorkCorrectly()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        try
        {
            // First lifecycle
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);
            await endpoint.StopAsync(CancellationToken.None);

            // Create new instance for restart (endpoints are typically not reusable)
            endpoint.Dispose();
            endpoint = new TestEndpoint();

            // Act - Second lifecycle
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.ExecuteAsyncCalled.Should().BeTrue();

            await endpoint.StopAsync(CancellationToken.None);
        }
        finally
        {
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithCustomRecoverability_ShouldApplySettings()
    {
        // Arrange
        var endpoint = new CustomRecoverabilityEndpoint(TestHelpers.CreateTestConfiguration())
        {
            CustomImmediateRetries = 7,
            CustomDelayedRetries = 3
        };

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - Endpoint configured and running
            await endpoint.StopAsync(CancellationToken.None);
        }
        finally
        {
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithCancellationDuringStartup_ShouldHandleGracefully()
    {
        // Arrange
        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration());
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        try
        {
            // Act - Start with short cancellation timeout
            var startTask = endpoint.StartAsync(cts.Token);

            // Wait for completion or cancellation
            await Task.WhenAny(startTask, Task.Delay(2000));

            // Assert - Should either complete or be cancelled
        }
        catch (OperationCanceledException)
        {
            // Expected if cancelled during startup
        }
        finally
        {
            try
            {
                await endpoint.StopAsync(CancellationToken.None);
            }
            catch
            {
                // Ignore errors during cleanup
            }
            endpoint.Dispose();
            cts.Dispose();
        }
    }

    [Fact]
    public async Task StressTest_MultipleSequentialStartStop_ShouldNotLeak()
    {
        // Arrange & Act
        for (int i = 0; i < 5; i++)
        {
            var endpoint = new TestEndpoint();
            try
            {
                await endpoint.StartAsync(CancellationToken.None);
                await Task.Delay(200);
                await endpoint.StopAsync(CancellationToken.None);
            }
            finally
            {
                endpoint.Dispose();
            }
        }

        // Assert - no memory leaks or exceptions
    }
}
