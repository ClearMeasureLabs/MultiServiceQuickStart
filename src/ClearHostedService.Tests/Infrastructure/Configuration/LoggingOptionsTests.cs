using ClearMeasure.HostedService.Configuration;
using FluentAssertions;
using Serilog;
using Serilog.Events;
using Xunit;

namespace QuickHostedService.Tests.Infrastructure.Configuration;

public class LoggingOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        var options = new LoggingOptions();

        options.LogLevel.Should().Be(LogEventLevel.Information);
        options.LogDirectory.Should().Be("logs");
        options.RollingInterval.Should().Be(RollingInterval.Day);
        options.EnableConsoleLogging.Should().BeTrue();
        options.EnableFileLogging.Should().BeTrue();
        options.EnableApplicationInsights.Should().BeFalse();
        options.ApplicationInsightsConnectionString.Should().BeNull();
        options.OutputTemplate.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ApplicationInsightsConnectionString_CanBeSet()
    {
        var options = new LoggingOptions
        {
            ApplicationInsightsConnectionString = "InstrumentationKey=12345678-1234-1234-1234-123456789012"
        };

        options.ApplicationInsightsConnectionString.Should().Be("InstrumentationKey=12345678-1234-1234-1234-123456789012");
    }

    [Fact]
    public void EnableApplicationInsights_CanBeSet()
    {
        var options = new LoggingOptions
        {
            EnableApplicationInsights = true
        };

        options.EnableApplicationInsights.Should().BeTrue();
    }

    [Fact]
    public void ApplicationInsightsConnectionString_CanBeNull()
    {
        var options = new LoggingOptions
        {
            ApplicationInsightsConnectionString = null
        };

        options.ApplicationInsightsConnectionString.Should().BeNull();
    }

    [Fact]
    public void LogLevel_CanBeChanged()
    {
        var options = new LoggingOptions
        {
            LogLevel = LogEventLevel.Debug
        };

        options.LogLevel.Should().Be(LogEventLevel.Debug);
    }

    [Fact]
    public void EnableConsoleLogging_CanBeDisabled()
    {
        var options = new LoggingOptions
        {
            EnableConsoleLogging = false
        };

        options.EnableConsoleLogging.Should().BeFalse();
    }

    [Fact]
    public void EnableFileLogging_CanBeDisabled()
    {
        var options = new LoggingOptions
        {
            EnableFileLogging = false
        };

        options.EnableFileLogging.Should().BeFalse();
    }
}
