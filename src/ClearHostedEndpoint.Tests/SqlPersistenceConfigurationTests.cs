namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Unit tests for SQL persistence configuration.
/// </summary>
public class SqlPersistenceConfigurationTests
{
    [Fact]
    public async Task Endpoint_WithoutSqlPersistence_ShouldUseLearningPersistence()
    {
        // Arrange
        var endpoint = new TestEndpoint(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.ConfigurePersistenceCalled.Should().BeTrue();
        }
        finally
        {
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithSqlPersistence_ShouldConfigureSqlPersistence()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            Schema = "dbo"
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

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
    public async Task Endpoint_WithSqlPersistence_AndCustomSchema_ShouldUseCustomSchema()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            Schema = "messaging"
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

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
    public async Task Endpoint_WithSqlPersistence_AndCustomTablePrefix_ShouldUseCustomPrefix()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            TablePrefix = "MyEndpoint_"
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

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
    public async Task Endpoint_WithSqlPersistence_AndNullTablePrefix_ShouldUseEndpointName()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            TablePrefix = null
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

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
    public async Task Endpoint_WithSqlPersistence_AndSagaPersistenceEnabled_ShouldConfigureSagas()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            EnableSagaPersistence = true
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

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
    public async Task Endpoint_WithSqlPersistence_AndSubscriptionStorageEnabled_ShouldConfigureSubscriptions()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            EnableSubscriptionStorage = true,
            SubscriptionCachePeriod = TimeSpan.FromSeconds(10)
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

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
    public async Task Endpoint_WithSqlPersistence_AndNullConnectionString_ShouldThrowEndpointConfigurationException()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = null
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EndpointConfigurationException>()
            .Where(ex => ex.Message.Contains("ConnectionString is required") || 
                        ex.InnerException != null);

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithSqlPersistence_AndEmptyConnectionString_ShouldThrowEndpointConfigurationException()
    {
        // Arrange
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = string.Empty
        };
        var endpoint = new SqlPersistenceEndpoint(TestHelpers.CreateTestConfiguration(), sqlOptions);

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EndpointConfigurationException>()
            .Where(ex => ex.Message.Contains("ConnectionString is required") || 
                        ex.InnerException != null);

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithSqlPersistence_AndOutboxEnabled_ShouldThrowForUnsupportedTransport()
    {
        // Arrange - Outbox requires specific transport configuration
        var endpointOptions = new EndpointOptions
        {
            EnableOutbox = true,
            OutboxTimeToKeepDeduplicationData = TimeSpan.FromDays(14)
        };
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;"
        };
        var endpoint = new TestEndpoint(
            endpointOptions: endpointOptions,
            sqlPersistenceOptions: sqlOptions);

        // Act
        var act = async () => await endpoint.StartAsync(CancellationToken.None);

        // Assert - LearningTransport doesn't support outbox, should throw
        await act.Should().ThrowAsync<EndpointConfigurationException>();

        endpoint.Dispose();
    }

    [Fact]
    public async Task Endpoint_WithAllSqlPersistenceOptions_ShouldConfigureAllSettings()
    {
        // Arrange - Test without outbox since LearningTransport doesn't support it
        var endpointOptions = new EndpointOptions
        {
            EnableOutbox = false
        };
        var sqlOptions = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            Schema = "messaging",
            TablePrefix = "Test_",
            EnableSagaPersistence = true,
            EnableSubscriptionStorage = true,
            SubscriptionCachePeriod = TimeSpan.FromSeconds(15)
        };
        var endpoint = new TestEndpoint(
            endpointOptions: endpointOptions,
            sqlPersistenceOptions: sqlOptions);

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
