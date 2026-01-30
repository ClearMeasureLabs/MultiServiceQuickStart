namespace ClearHostedEndpoint.Tests.Configuration;

/// <summary>
/// Unit tests for EndpointOptions configuration class.
/// </summary>
public class EndpointOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var options = new EndpointOptions();

        // Assert
        options.EndpointName.Should().BeNull();
        options.EnableInstallers.Should().BeTrue();
        options.PurgeOnStartup.Should().BeFalse();
        options.ErrorQueue.Should().Be("error");
        options.AuditQueue.Should().BeNull();
        options.EnableMetrics.Should().BeFalse();
        options.ImmediateRetryCount.Should().Be(3);
        options.DelayedRetryCount.Should().Be(3);
        options.DelayedRetryTimeIncrease.Should().Be(TimeSpan.FromSeconds(10));
        options.MaxConcurrency.Should().Be(1);
        options.EnableOutbox.Should().BeFalse();
        options.OutboxCleanupBatchSize.Should().Be(100);
        options.OutboxTimeToKeepDeduplicationData.Should().Be(TimeSpan.FromDays(7));
    }

    [Fact]
    public void EndpointName_CanBeSet()
    {
        // Arrange
        var options = new EndpointOptions();
        var expectedName = "TestEndpoint";

        // Act
        options.EndpointName = expectedName;

        // Assert
        options.EndpointName.Should().Be(expectedName);
    }

    [Fact]
    public void EnableInstallers_CanBeSet()
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.EnableInstallers = false;

        // Assert
        options.EnableInstallers.Should().BeFalse();
    }

    [Fact]
    public void PurgeOnStartup_CanBeSet()
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.PurgeOnStartup = true;

        // Assert
        options.PurgeOnStartup.Should().BeTrue();
    }

    [Fact]
    public void ErrorQueue_CanBeCustomized()
    {
        // Arrange
        var options = new EndpointOptions();
        var customErrorQueue = "custom-error";

        // Act
        options.ErrorQueue = customErrorQueue;

        // Assert
        options.ErrorQueue.Should().Be(customErrorQueue);
    }

    [Fact]
    public void AuditQueue_CanBeSet()
    {
        // Arrange
        var options = new EndpointOptions();
        var auditQueue = "audit";

        // Act
        options.AuditQueue = auditQueue;

        // Assert
        options.AuditQueue.Should().Be(auditQueue);
    }

    [Fact]
    public void EnableMetrics_CanBeSet()
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.EnableMetrics = true;

        // Assert
        options.EnableMetrics.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void ImmediateRetryCount_CanBeSet(int retryCount)
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.ImmediateRetryCount = retryCount;

        // Assert
        options.ImmediateRetryCount.Should().Be(retryCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void DelayedRetryCount_CanBeSet(int retryCount)
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.DelayedRetryCount = retryCount;

        // Assert
        options.DelayedRetryCount.Should().Be(retryCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(60)]
    [InlineData(300)]
    public void DelayedRetryTimeIncrease_CanBeSet(int seconds)
    {
        // Arrange
        var options = new EndpointOptions();
        var timeSpan = TimeSpan.FromSeconds(seconds);

        // Act
        options.DelayedRetryTimeIncrease = timeSpan;

        // Assert
        options.DelayedRetryTimeIncrease.Should().Be(timeSpan);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void MaxConcurrency_CanBeSet(int concurrency)
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.MaxConcurrency = concurrency;

        // Assert
        options.MaxConcurrency.Should().Be(concurrency);
    }

    [Fact]
    public void EnableOutbox_CanBeSet()
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.EnableOutbox = true;

        // Assert
        options.EnableOutbox.Should().BeTrue();
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void OutboxCleanupBatchSize_CanBeSet(int batchSize)
    {
        // Arrange
        var options = new EndpointOptions();

        // Act
        options.OutboxCleanupBatchSize = batchSize;

        // Assert
        options.OutboxCleanupBatchSize.Should().Be(batchSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    public void OutboxTimeToKeepDeduplicationData_CanBeSet(int days)
    {
        // Arrange
        var options = new EndpointOptions();
        var timeSpan = TimeSpan.FromDays(days);

        // Act
        options.OutboxTimeToKeepDeduplicationData = timeSpan;

        // Assert
        options.OutboxTimeToKeepDeduplicationData.Should().Be(timeSpan);
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        // Arrange & Act
        var options = new EndpointOptions
        {
            EndpointName = "TestEndpoint",
            EnableInstallers = false,
            PurgeOnStartup = true,
            ErrorQueue = "custom-error",
            AuditQueue = "custom-audit",
            EnableMetrics = true,
            ImmediateRetryCount = 5,
            DelayedRetryCount = 10,
            DelayedRetryTimeIncrease = TimeSpan.FromSeconds(30),
            MaxConcurrency = 10,
            EnableOutbox = true,
            OutboxCleanupBatchSize = 200,
            OutboxTimeToKeepDeduplicationData = TimeSpan.FromDays(14)
        };

        // Assert
        options.EndpointName.Should().Be("TestEndpoint");
        options.EnableInstallers.Should().BeFalse();
        options.PurgeOnStartup.Should().BeTrue();
        options.ErrorQueue.Should().Be("custom-error");
        options.AuditQueue.Should().Be("custom-audit");
        options.EnableMetrics.Should().BeTrue();
        options.ImmediateRetryCount.Should().Be(5);
        options.DelayedRetryCount.Should().Be(10);
        options.DelayedRetryTimeIncrease.Should().Be(TimeSpan.FromSeconds(30));
        options.MaxConcurrency.Should().Be(10);
        options.EnableOutbox.Should().BeTrue();
        options.OutboxCleanupBatchSize.Should().Be(200);
        options.OutboxTimeToKeepDeduplicationData.Should().Be(TimeSpan.FromDays(14));
    }
}
