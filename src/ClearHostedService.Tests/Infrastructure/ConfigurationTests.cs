using ClearMeasure.HostedService.Configuration;
using FluentAssertions;
using Serilog;
using Serilog.Events;
using Xunit;

namespace QuickHostedService.Tests.Infrastructure;

public class LoggingOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new LoggingOptions();

        // Assert
        options.LogLevel.Should().Be(LogEventLevel.Information);
        options.LogDirectory.Should().Be("logs");
        options.RollingInterval.Should().Be(RollingInterval.Day);
        options.EnableConsoleLogging.Should().BeTrue();
        options.EnableFileLogging.Should().BeTrue();
        options.EnableApplicationInsights.Should().BeFalse();
        options.ApplicationInsightsConnectionString.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new LoggingOptions();

        // Act
        options.LogLevel = LogEventLevel.Debug;
        options.LogDirectory = "custom-logs";
        options.RollingInterval = RollingInterval.Hour;
        options.EnableConsoleLogging = false;
        options.EnableFileLogging = false;
        options.EnableApplicationInsights = true;
        options.ApplicationInsightsConnectionString = "InstrumentationKey=test-key";

        // Assert
        options.LogLevel.Should().Be(LogEventLevel.Debug);
        options.LogDirectory.Should().Be("custom-logs");
        options.RollingInterval.Should().Be(RollingInterval.Hour);
        options.EnableConsoleLogging.Should().BeFalse();
        options.EnableFileLogging.Should().BeFalse();
        options.EnableApplicationInsights.Should().BeTrue();
        options.ApplicationInsightsConnectionString.Should().Be("InstrumentationKey=test-key");
    }
}

public class HostedServiceOptionsTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new HostedServiceOptions();

        // Assert
        options.ShutdownTimeout.Should().Be(TimeSpan.FromSeconds(30));
        options.EnableDetailedErrors.Should().BeFalse();
        options.ServiceName.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new HostedServiceOptions();

        // Act
        options.ShutdownTimeout = TimeSpan.FromMinutes(5);
        options.EnableDetailedErrors = true;
        options.ServiceName = "TestService";

        // Assert
        options.ShutdownTimeout.Should().Be(TimeSpan.FromMinutes(5));
        options.EnableDetailedErrors.Should().BeTrue();
        options.ServiceName.Should().Be("TestService");
    }
}
