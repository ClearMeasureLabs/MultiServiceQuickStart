using ClearMeasure.HostedService.Infrastructure;
using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace QuickHostedService.Tests.Infrastructure;

public class CloudRoleNameTelemetryConverterTests
{
    [Fact]
    public void Constructor_WhenCloudRoleNameIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CloudRoleNameTelemetryConverter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cloudRoleName");
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleName()
    {
        // Arrange
        const string expectedRoleName = "TestService";
        var converter = new CloudRoleNameTelemetryConverter(expectedRoleName);
        var logEvent = CreateLogEvent();

        // Act
        var telemetryItems = converter.Convert(logEvent, null!).ToList();

        // Assert
        telemetryItems.Should().NotBeEmpty();
        foreach (var telemetry in telemetryItems)
        {
            telemetry.Context.Cloud.RoleName.Should().Be(expectedRoleName);
        }
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());
    }
}

public class CloudRoleInstanceTelemetryConverterTests
{
    [Fact]
    public void Constructor_WhenCloudRoleInstanceIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CloudRoleInstanceTelemetryConverter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cloudRoleInstance");
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleInstance()
    {
        // Arrange
        const string expectedRoleInstance = "TestMachine_Instance1";
        var converter = new CloudRoleInstanceTelemetryConverter(expectedRoleInstance);
        var logEvent = CreateLogEvent();

        // Act
        var telemetryItems = converter.Convert(logEvent, null!).ToList();

        // Assert
        telemetryItems.Should().NotBeEmpty();
        foreach (var telemetry in telemetryItems)
        {
            telemetry.Context.Cloud.RoleInstance.Should().Be(expectedRoleInstance);
        }
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());
    }
}

public class CompositeTelemetryConverterTests
{
    [Fact]
    public void Constructor_WhenCloudRoleNameIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CompositeTelemetryConverter(null!, "instance");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cloudRoleName");
    }

    [Fact]
    public void Constructor_WhenCloudRoleInstanceIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CompositeTelemetryConverter("role", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cloudRoleInstance");
    }

    [Fact]
    public void Convert_ShouldSetBothCloudRoleNameAndInstance()
    {
        // Arrange
        const string expectedRoleName = "TestService";
        const string expectedRoleInstance = "TestMachine_Instance1";
        var converter = new CompositeTelemetryConverter(expectedRoleName, expectedRoleInstance);
        var logEvent = CreateLogEvent();

        // Act
        var telemetryItems = converter.Convert(logEvent, null!).ToList();

        // Assert
        telemetryItems.Should().NotBeEmpty();
        foreach (var telemetry in telemetryItems)
        {
            telemetry.Context.Cloud.RoleName.Should().Be(expectedRoleName);
            telemetry.Context.Cloud.RoleInstance.Should().Be(expectedRoleInstance);
        }
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            new MessageTemplate("Test message", Enumerable.Empty<MessageTemplateToken>()),
            Enumerable.Empty<LogEventProperty>());
    }
}
