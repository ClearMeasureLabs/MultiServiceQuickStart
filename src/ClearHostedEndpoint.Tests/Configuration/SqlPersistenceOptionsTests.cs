namespace ClearHostedEndpoint.Tests.Configuration;

/// <summary>
/// Unit tests for SqlPersistenceOptions configuration class.
/// </summary>
public class SqlPersistenceOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var options = new SqlPersistenceOptions();

        // Assert
        options.ConnectionString.Should().BeNull();
        options.Schema.Should().Be("dbo");
        options.TablePrefix.Should().BeNull();
        options.EnableSagaPersistence.Should().BeTrue();
        options.EnableSubscriptionStorage.Should().BeFalse();
        options.SubscriptionCachePeriod.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ConnectionString_CanBeSet()
    {
        // Arrange
        var options = new SqlPersistenceOptions();
        var connectionString = "Server=localhost;Database=Test;Trusted_Connection=true;";

        // Act
        options.ConnectionString = connectionString;

        // Assert
        options.ConnectionString.Should().Be(connectionString);
    }

    [Theory]
    [InlineData("dbo")]
    [InlineData("nsb")]
    [InlineData("messaging")]
    [InlineData("custom")]
    public void Schema_CanBeSet(string schema)
    {
        // Arrange
        var options = new SqlPersistenceOptions();

        // Act
        options.Schema = schema;

        // Assert
        options.Schema.Should().Be(schema);
    }

    [Theory]
    [InlineData("MyEndpoint_")]
    [InlineData("NSB_")]
    [InlineData("Test_")]
    [InlineData(null)]
    public void TablePrefix_CanBeSet(string? tablePrefix)
    {
        // Arrange
        var options = new SqlPersistenceOptions();

        // Act
        options.TablePrefix = tablePrefix;

        // Assert
        options.TablePrefix.Should().Be(tablePrefix);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableSagaPersistence_CanBeSet(bool enabled)
    {
        // Arrange
        var options = new SqlPersistenceOptions();

        // Act
        options.EnableSagaPersistence = enabled;

        // Assert
        options.EnableSagaPersistence.Should().Be(enabled);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableSubscriptionStorage_CanBeSet(bool enabled)
    {
        // Arrange
        var options = new SqlPersistenceOptions();

        // Act
        options.EnableSubscriptionStorage = enabled;

        // Assert
        options.EnableSubscriptionStorage.Should().Be(enabled);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(60)]
    public void SubscriptionCachePeriod_CanBeSet(int seconds)
    {
        // Arrange
        var options = new SqlPersistenceOptions();
        var timeSpan = TimeSpan.FromSeconds(seconds);

        // Act
        options.SubscriptionCachePeriod = timeSpan;

        // Assert
        options.SubscriptionCachePeriod.Should().Be(timeSpan);
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        // Arrange & Act
        var options = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=Test;Trusted_Connection=true;",
            Schema = "messaging",
            TablePrefix = "MyEndpoint_",
            EnableSagaPersistence = true,
            EnableSubscriptionStorage = true,
            SubscriptionCachePeriod = TimeSpan.FromSeconds(10)
        };

        // Assert
        options.ConnectionString.Should().Be("Server=localhost;Database=Test;Trusted_Connection=true;");
        options.Schema.Should().Be("messaging");
        options.TablePrefix.Should().Be("MyEndpoint_");
        options.EnableSagaPersistence.Should().BeTrue();
        options.EnableSubscriptionStorage.Should().BeTrue();
        options.SubscriptionCachePeriod.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ConnectionString_CanBeSetToNull()
    {
        // Arrange
        var options = new SqlPersistenceOptions
        {
            ConnectionString = "Server=localhost;Database=Test;"
        };

        // Act
        options.ConnectionString = null;

        // Assert
        options.ConnectionString.Should().BeNull();
    }

    [Fact]
    public void Schema_DefaultValue_ShouldNotBeNull()
    {
        // Arrange & Act
        var options = new SqlPersistenceOptions();

        // Assert
        options.Schema.Should().NotBeNullOrEmpty();
        options.Schema.Should().Be("dbo");
    }
}
