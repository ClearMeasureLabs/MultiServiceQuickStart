using ClearMeasure.HostedService.Infrastructure.TelemetryConverters;
using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;

namespace QuickHostedService.Tests.Infrastructure;

public class TelemetryConverterTests
{
    [Fact]
    public void CloudRoleNameConverter_SetsRoleName_WhenInitialized()
    {
        // Arrange
        var roleName = "TestService";
        var converter = new CloudRoleNameConverter(roleName);
        var telemetry = new TraceTelemetry();

        // Act
        converter.Initialize(telemetry);

        // Assert
        telemetry.Context.Cloud.RoleName.Should().Be(roleName);
    }

    [Fact]
    public void CloudRoleNameConverter_ThrowsArgumentNullException_WhenRoleNameIsNull()
    {
        // Act & Assert
        Action act = () => new CloudRoleNameConverter(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CloudRoleNameConverter_DoesNotThrow_WhenTelemetryIsNull()
    {
        // Arrange
        var converter = new CloudRoleNameConverter("TestService");

        // Act & Assert
        Action act = () => converter.Initialize(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void CloudInstanceConverter_SetsRoleInstance_WhenInitialized()
    {
        // Arrange
        var cloudInstance = "TestInstance";
        var converter = new CloudInstanceConverter(cloudInstance);
        var telemetry = new TraceTelemetry();

        // Act
        converter.Initialize(telemetry);

        // Assert
        telemetry.Context.Cloud.RoleInstance.Should().Be(cloudInstance);
    }

    [Fact]
    public void CloudInstanceConverter_ThrowsArgumentNullException_WhenCloudInstanceIsNull()
    {
        // Act & Assert
        Action act = () => new CloudInstanceConverter(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CloudInstanceConverter_DoesNotThrow_WhenTelemetryIsNull()
    {
        // Arrange
        var converter = new CloudInstanceConverter("TestInstance");

        // Act & Assert
        Action act = () => converter.Initialize(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void CloudRoleNameConverter_WorksWithDifferentTelemetryTypes()
    {
        // Arrange
        var roleName = "TestService";
        var converter = new CloudRoleNameConverter(roleName);
        var traceTelemetry = new TraceTelemetry();
        var requestTelemetry = new RequestTelemetry();
        var dependencyTelemetry = new DependencyTelemetry();

        // Act
        converter.Initialize(traceTelemetry);
        converter.Initialize(requestTelemetry);
        converter.Initialize(dependencyTelemetry);

        // Assert
        traceTelemetry.Context.Cloud.RoleName.Should().Be(roleName);
        requestTelemetry.Context.Cloud.RoleName.Should().Be(roleName);
        dependencyTelemetry.Context.Cloud.RoleName.Should().Be(roleName);
    }

    [Fact]
    public void CloudInstanceConverter_WorksWithDifferentTelemetryTypes()
    {
        // Arrange
        var cloudInstance = "ResourceGroup/Instance1";
        var converter = new CloudInstanceConverter(cloudInstance);
        var traceTelemetry = new TraceTelemetry();
        var requestTelemetry = new RequestTelemetry();
        var dependencyTelemetry = new DependencyTelemetry();

        // Act
        converter.Initialize(traceTelemetry);
        converter.Initialize(requestTelemetry);
        converter.Initialize(dependencyTelemetry);

        // Assert
        traceTelemetry.Context.Cloud.RoleInstance.Should().Be(cloudInstance);
        requestTelemetry.Context.Cloud.RoleInstance.Should().Be(cloudInstance);
        dependencyTelemetry.Context.Cloud.RoleInstance.Should().Be(cloudInstance);
    }
}
