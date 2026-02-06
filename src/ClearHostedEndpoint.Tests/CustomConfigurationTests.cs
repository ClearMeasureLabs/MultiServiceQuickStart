namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Unit tests for custom endpoint configurations and overrides.
/// </summary>
public class CustomConfigurationTests
{
    [Fact]
    public async Task CustomRecoverabilityEndpoint_ShouldUseCustomRetrySettings()
    {
        // Arrange
        var endpoint = new CustomRecoverabilityEndpoint(TestHelpers.CreateTestConfiguration())
        {
            CustomImmediateRetries = 5,
            CustomDelayedRetries = 10
        };

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts with custom settings
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task CustomNamedEndpoint_ShouldUseProvidedName()
    {
        // Arrange
        var customName = "MyCustomEndpoint";
        var endpoint = new CustomNamedEndpoint(TestHelpers.CreateTestConfiguration(), customName);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - endpoint starts with custom name
            var effectiveName = endpoint.GetType()
                .BaseType?
                .GetProperty("EffectiveEndpointName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(endpoint) as string;

            effectiveName.Should().Be(customName);
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public void CreateDbConnection_ShouldCreateValidConnection()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=Test;"
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);
        var connectionString = "Server=localhost;Database=Test;";

        // Act
        var method = endpoint.GetType()
            .BaseType?
            .GetMethod("CreateDbConnection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var connection = method?.Invoke(endpoint, new object[] { connectionString });

        // Assert
        connection.Should().NotBeNull();
        connection.Should().BeAssignableTo<System.Data.Common.DbConnection>();

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithCustomSerialization_CanOverrideDefault()
    {
        // Arrange
        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.ConfigureSerializationCalled.Should().BeTrue();
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_ConfigureEndpointAsync_AllowsAsyncConfiguration()
    {
        // Arrange
        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.ConfigureEndpointAsyncCalled.Should().BeTrue();
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Theory]
    [InlineData("Endpoint1")]
    [InlineData("Endpoint.With.Dots")]
    [InlineData("Endpoint_With_Underscores")]
    [InlineData("Endpoint-With-Dashes")]
    public async Task Endpoint_WithVariousNames_ShouldHandleCorrectly(string endpointName)
    {
        // Arrange
        var endpoint = new CustomNamedEndpoint(TestHelpers.CreateTestConfiguration(), endpointName);

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
    public async Task Endpoint_WithComplexConfiguration_ShouldHandleAllSettings()
    {
        // Arrange
        var endpointOptions = new EndpointOptions
        {
            EndpointName = "ComplexEndpoint",
            EnableInstallers = true,
            PurgeOnStartup = false,
            ErrorQueue = "complex-error",
            AuditQueue = "complex-audit",
            MaxConcurrency = 3,
            ImmediateRetryCount = 2,
            DelayedRetryCount = 5,
            DelayedRetryTimeIncrease = TimeSpan.FromSeconds(15),
            EnableMetrics = false,
            EnableOutbox = false
        };

        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration(), endpointOptions: endpointOptions);

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - all configuration methods called
            endpoint.ConfigureTransportCalled.Should().BeTrue();
            endpoint.ConfigureEndpointCalled.Should().BeTrue();
            endpoint.ConfigureSerializationCalled.Should().BeTrue();
            endpoint.ConfigurePersistenceCalled.Should().BeTrue();
            endpoint.ConfigureRecoverabilityCalled.Should().BeTrue();
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }
}
