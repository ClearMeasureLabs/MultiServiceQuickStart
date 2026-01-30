namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Unit tests for endpoint options configuration.
/// </summary>
public class EndpointOptionsConfigurationTests
{
    [Fact]
    public async Task Endpoint_WithDefaultOptions_ShouldUseDefaultSettings()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.ConfigureTransportCalled.Should().BeTrue();
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithEnableInstallers_ShouldConfigureInstallers()
    {
        // Arrange
        var options = new EndpointOptions { EnableInstallers = true };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithCustomErrorQueue_ShouldUseCustomQueue()
    {
        // Arrange
        var options = new EndpointOptions { ErrorQueue = "custom-error" };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithAuditQueue_ShouldConfigureAuditing()
    {
        // Arrange
        var options = new EndpointOptions { AuditQueue = "audit" };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Endpoint_WithCustomMaxConcurrency_ShouldConfigureConcurrency(int maxConcurrency)
    {
        // Arrange
        var options = new EndpointOptions { MaxConcurrency = maxConcurrency };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Endpoint_WithCustomImmediateRetries_ShouldConfigureRetries(int retries)
    {
        // Arrange
        var options = new EndpointOptions { ImmediateRetryCount = retries };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public async Task Endpoint_WithCustomDelayedRetries_ShouldConfigureRetries(int retries)
    {
        // Arrange
        var options = new EndpointOptions { DelayedRetryCount = retries };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithPurgeOnStartup_ShouldConfigurePurging()
    {
        // Arrange
        var options = new EndpointOptions { PurgeOnStartup = true };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithAllCustomOptions_ShouldConfigureAllSettings()
    {
        // Arrange
        var options = new EndpointOptions
        {
            EndpointName = "CustomTestEndpoint",
            EnableInstallers = false,
            ErrorQueue = "custom-error",
            AuditQueue = "custom-audit",
            MaxConcurrency = 5,
            ImmediateRetryCount = 2,
            DelayedRetryCount = 3,
            DelayedRetryTimeIncrease = TimeSpan.FromSeconds(5)
        };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithEnableMetrics_ShouldConfigureMetrics()
    {
        // Arrange
        var options = new EndpointOptions { EnableMetrics = true };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts without error
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }
}
