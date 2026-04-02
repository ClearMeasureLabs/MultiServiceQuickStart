using ClearMeasure.HostedService.Telemetry;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Xunit;

namespace QuickHostedService.Tests.Infrastructure.Telemetry;

public class TelemetryConverterTests
{
    [Fact]
    public void Constructor_WithValidApplicationName_ShouldNotThrow()
    {
        var act = () => new TelemetryConverter("TestApp");

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullApplicationName_ShouldThrowArgumentNullException()
    {
        var act = () => new TelemetryConverter(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("applicationName");
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleName()
    {
        var converter = new TelemetryConverter("TestApplication");
        var logEvent = CreateLogEvent("Test message");

        var telemetries = converter.Convert(logEvent, null!);

        var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
        traceTelemetry.Should().NotBeNull();
        traceTelemetry!.Context.Cloud.RoleName.Should().Be("TestApplication");
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleInstance_FromMachineName()
    {
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
        Environment.SetEnvironmentVariable("WEBSITE_INSTANCE_ID", null);
        Environment.SetEnvironmentVariable("WEBSITE_RESOURCE_GROUP", null);
        Environment.SetEnvironmentVariable("HOSTNAME", null);

        var converter = new TelemetryConverter("TestApp");
        var logEvent = CreateLogEvent("Test message");

        var telemetries = converter.Convert(logEvent, null!);

        var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
        traceTelemetry.Should().NotBeNull();
        traceTelemetry!.Context.Cloud.RoleInstance.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleInstance_FromAzureAppService_WithResourceGroup()
    {
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "MyAppService");
        Environment.SetEnvironmentVariable("WEBSITE_RESOURCE_GROUP", "MyResourceGroup");

        try
        {
            var converter = new TelemetryConverter("TestApp");
            var logEvent = CreateLogEvent("Test message");

            var telemetries = converter.Convert(logEvent, null!);

            var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
            traceTelemetry.Should().NotBeNull();
            traceTelemetry!.Context.Cloud.RoleInstance.Should().Be("MyResourceGroup/MyAppService");
        }
        finally
        {
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            Environment.SetEnvironmentVariable("WEBSITE_RESOURCE_GROUP", null);
        }
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleInstance_FromAzureAppService_WithInstanceId()
    {
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "MyAppService");
        Environment.SetEnvironmentVariable("WEBSITE_INSTANCE_ID", "instance123");
        Environment.SetEnvironmentVariable("WEBSITE_RESOURCE_GROUP", null);

        try
        {
            var converter = new TelemetryConverter("TestApp");
            var logEvent = CreateLogEvent("Test message");

            var telemetries = converter.Convert(logEvent, null!);

            var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
            traceTelemetry.Should().NotBeNull();
            traceTelemetry!.Context.Cloud.RoleInstance.Should().Be("MyAppService/instance123");
        }
        finally
        {
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            Environment.SetEnvironmentVariable("WEBSITE_INSTANCE_ID", null);
        }
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleInstance_FromAzureAppService_WithOnlySiteName()
    {
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "MyAppService");
        Environment.SetEnvironmentVariable("WEBSITE_INSTANCE_ID", null);
        Environment.SetEnvironmentVariable("WEBSITE_RESOURCE_GROUP", null);

        try
        {
            var converter = new TelemetryConverter("TestApp");
            var logEvent = CreateLogEvent("Test message");

            var telemetries = converter.Convert(logEvent, null!);

            var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
            traceTelemetry.Should().NotBeNull();
            traceTelemetry!.Context.Cloud.RoleInstance.Should().Be("MyAppService");
        }
        finally
        {
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
        }
    }

    [Fact]
    public void Convert_ShouldSetCloudRoleInstance_FromDockerContainer()
    {
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
        Environment.SetEnvironmentVariable("HOSTNAME", "container-abcd1234efgh5678");

        try
        {
            var converter = new TelemetryConverter("TestApp");
            var logEvent = CreateLogEvent("Test message");

            var telemetries = converter.Convert(logEvent, null!);

            var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
            traceTelemetry.Should().NotBeNull();
            traceTelemetry!.Context.Cloud.RoleInstance.Should().Be("container-abcd1234efgh5678");
        }
        finally
        {
            Environment.SetEnvironmentVariable("HOSTNAME", null);
        }
    }

    [Fact]
    public void Convert_ShouldIgnoreShortHostname()
    {
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
        Environment.SetEnvironmentVariable("HOSTNAME", "short");

        try
        {
            var converter = new TelemetryConverter("TestApp");
            var logEvent = CreateLogEvent("Test message");

            var telemetries = converter.Convert(logEvent, null!);

            var traceTelemetry = telemetries.OfType<TraceTelemetry>().FirstOrDefault();
            traceTelemetry.Should().NotBeNull();
            traceTelemetry!.Context.Cloud.RoleInstance.Should().Be(Environment.MachineName);
        }
        finally
        {
            Environment.SetEnvironmentVariable("HOSTNAME", null);
        }
    }

    private static LogEvent CreateLogEvent(string message)
    {
        var messageTemplate = new Serilog.Events.MessageTemplate(message, Array.Empty<Serilog.Parsing.MessageTemplateToken>());
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            messageTemplate,
            Array.Empty<LogEventProperty>());
    }
}
