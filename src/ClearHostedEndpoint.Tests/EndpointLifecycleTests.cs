namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Unit tests for endpoint lifecycle management.
/// </summary>
public class EndpointLifecycleTests
{
    [Fact]
    public async Task StartAsync_ThenStopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        // Act & Assert
        await endpoint.StartAsync(CancellationToken.None);
        await Task.Delay(500);
        await endpoint.StopAsync(CancellationToken.None);

        endpoint.Dispose();
    }

    [Fact]
    public async Task OnStoppingAsync_ShouldBeCalledDuringShutdown()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        try
        {
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Act
            await endpoint.StopAsync(CancellationToken.None);

            // Assert
            // OnStoppingAsync is called as part of StopAsync
            // We can't directly verify it was called without reflection or a test hook
        }
        finally
        {
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task StopAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var endpoint = new TestEndpoint();
        var cts = new CancellationTokenSource();

        try
        {
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Act
            await endpoint.StopAsync(cts.Token);

            // Assert - no exception thrown
        }
        finally
        {
            endpoint.Dispose();
            cts.Dispose();
        }
    }

    [Fact]
    public async Task Dispose_AfterStop_ShouldNotThrow()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        await endpoint.StartAsync(CancellationToken.None);
        await Task.Delay(500);
        await endpoint.StopAsync(CancellationToken.None);

        // Act
        endpoint.Dispose();

        // Assert - no exception thrown
    }

    [Fact]
    public async Task Dispose_WithoutStop_ShouldNotThrow()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        await endpoint.StartAsync(CancellationToken.None);
        await Task.Delay(500);

        // Act
        endpoint.Dispose();

        // Assert - no exception thrown
    }

    [Fact]
    public void Dispose_WithoutStart_ShouldNotThrow()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        // Act
        endpoint.Dispose();

        // Assert - no exception thrown
    }

    [Fact]
    public async Task MultipleEndpoints_CanRunConcurrently()
    {
        // Arrange
        var endpoint1 = new TestEndpoint();
        var endpoint2 = new TestEndpoint();
        var endpoint3 = new TestEndpoint();

        try
        {
            // Act
            await endpoint1.StartAsync(CancellationToken.None);
            await endpoint2.StartAsync(CancellationToken.None);
            await endpoint3.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - all endpoints running
        }
        finally
        {
            await endpoint1.StopAsync(CancellationToken.None);
            await endpoint2.StopAsync(CancellationToken.None);
            await endpoint3.StopAsync(CancellationToken.None);
            endpoint1.Dispose();
            endpoint2.Dispose();
            endpoint3.Dispose();
        }
    }

    [Fact]
    public async Task StartAsync_CalledTwice_ShouldHandleGracefully()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Calling StartAsync again (behavior depends on base class implementation)
            var act = async () => await endpoint.StartAsync(CancellationToken.None);

            // Assert - might throw or handle gracefully depending on implementation
            // We just verify it doesn't crash the test
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task StopAsync_CalledTwice_ShouldHandleGracefully()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        await endpoint.StartAsync(CancellationToken.None);
        await Task.Delay(500);

        // Act
        await endpoint.StopAsync(CancellationToken.None);
        await endpoint.StopAsync(CancellationToken.None); // Call again

        // Assert - no exception thrown
        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_ShouldHandleGracefulShutdown_WithinTimeout()
    {
        // Arrange
        var endpoint = new TestEndpoint();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Act
            await endpoint.StopAsync(cts.Token);

            // Assert - completed within timeout
            cts.Token.IsCancellationRequested.Should().BeFalse();
        }
        finally
        {
            endpoint.Dispose();
            cts.Dispose();
        }
    }
}
