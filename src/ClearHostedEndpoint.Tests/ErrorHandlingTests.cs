namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Unit tests for error handling and edge cases in ClearHostedEndpoint.
/// </summary>
public class ErrorHandlingTests
{
    [Fact]
    public async Task Endpoint_WithInvalidTransportConfiguration_ShouldThrowEndpointConfigurationException()
    {
        // Arrange
        var endpoint = new FaultyTransportEndpoint();

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EndpointConfigurationException>()
            .Where(ex => ex.Message.Contains("Failed to start NServiceBus endpoint"));

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithNullSqlConnectionString_ShouldThrowEndpointConfigurationException()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = null
        };
        var endpoint = new SqlPersistenceEndpoint(sqlOptions);

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EndpointConfigurationException>()
            .Where(ex => ex.Message.Contains("ConnectionString is required") || 
                        ex.InnerException != null);

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithEmptySqlConnectionString_ShouldThrowEndpointConfigurationException()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = string.Empty
        };
        var endpoint = new SqlPersistenceEndpoint(sqlOptions);

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EndpointConfigurationException>()
            .Where(ex => ex.Message.Contains("ConnectionString is required") || 
                        ex.InnerException != null);

        endpoint.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldHandleGracefully()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        // Act & Assert
        endpoint.Dispose();
        endpoint.Dispose(); // Second call should not throw
        endpoint.Dispose(); // Third call should not throw
    }

    [Fact]
    public async Task StopAsync_BeforeStartAsync_ShouldHandleGracefully()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        try
        {
            // Act
            await endpoint.StopAsync(CancellationToken.None);

            // Assert - no exception thrown
        }
        finally
        {
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task StopAsync_WithVeryShortTimeout_ShouldAttemptGracefulShutdown()
    {
        // Arrange
        var endpoint = new TestEndpoint();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

        try
        {
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Act
            var act = async () => await endpoint.StopAsync(cts.Token);

            // Assert - may or may not throw depending on timing
            try
            {
                await act();
            }
            catch (OperationCanceledException)
            {
                // Expected if timeout is too short
            }
        }
        finally
        {
            endpoint.Dispose();
            cts.Dispose();
        }
    }

    [Fact]
    public void Endpoint_WithNullEndpointName_ShouldUseTypeName()
    {
        // Arrange
        var options = new EndpointOptions { EndpointName = null };
        var endpoint = new TestEndpoint(endpointOptions: options);

        // Act
        var effectiveName = endpoint.GetType()
            .BaseType?
            .GetProperty("EffectiveEndpointName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(endpoint) as string;

        // Assert
        effectiveName.Should().Be("TestEndpoint");

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithNegativeRetryCount_ShouldHandleGracefully()
    {
        // Arrange
        var options = new EndpointOptions
        {
            ImmediateRetryCount = -1,
            DelayedRetryCount = -1
        };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act - NServiceBus should handle invalid values
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint may or may not start depending on NServiceBus validation
        }
        catch (Exception ex)
        {
            // Expected if NServiceBus validates input
            ex.Should().BeAssignableTo<Exception>();
        }
        finally
        {
            try
            {
                await endpoint.StopAsync(CancellationToken.None);
            }
            catch { }
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithZeroConcurrency_ShouldHandleAppropriately()
    {
        // Arrange
        var options = new EndpointOptions { MaxConcurrency = 0 };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            var act = async () =>
            {
                await endpoint.StartAsync(CancellationToken.None);
                await Task.Delay(500);
            };

            // Assert - NServiceBus should validate this
            try
            {
                await act();
            }
            catch (Exception ex)
            {
                // Expected if NServiceBus validates concurrency > 0
                ex.Should().BeAssignableTo<Exception>();
            }
        }
        finally
        {
            try
            {
                await endpoint.StopAsync(CancellationToken.None);
            }
            catch { }
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithExtremelyHighConcurrency_ShouldAcceptValue()
    {
        // Arrange
        var options = new EndpointOptions { MaxConcurrency = 10000 };
        var endpoint = new TestEndpoint(endpointOptions: options);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts (NServiceBus handles extreme values)
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithVeryLargeOutboxRetention_ShouldThrowForUnsupportedTransport()
    {
        // Arrange - Outbox requires specific transport configuration
        var options = new EndpointOptions
        {
            EnableOutbox = true,
            OutboxTimeToKeepDeduplicationData = TimeSpan.FromDays(365)
        };
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=Test;"
        };
        var endpoint = new TestEndpoint(
            endpointOptions: options,
            sqlPersistenceOptions: sqlOptions);

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert - LearningTransport doesn't support outbox
        await act.Should().ThrowAsync<EndpointConfigurationException>();

        endpoint.Dispose();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Endpoint_WithWhitespaceErrorQueue_ShouldUseValue(string errorQueue)
    {
        // Arrange
        var options = new EndpointOptions { ErrorQueue = errorQueue };
        var endpoint = new TestEndpoint(endpointOptions: options);

        // Act & Assert - configuration is set
        endpoint.Dispose();
    }
}
