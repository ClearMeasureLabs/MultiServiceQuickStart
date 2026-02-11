using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace ClearMeasure.HostedService.Infrastructure;

/// <summary>
/// Telemetry initializer that sets the cloud role name and instance for Application Insights.
/// CloudRoleName is set to the application name.
/// CloudRoleInstance is set to the machine name for on-premises deployments,
/// or ResourceGroup+InstanceName for Azure/Docker deployments.
/// </summary>
public class CloudRoleTelemetryInitializer : ITelemetryInitializer
{
    private readonly string _roleName;
    private readonly string _roleInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudRoleTelemetryInitializer"/> class.
    /// </summary>
    /// <param name="roleName">The cloud role name (application name).</param>
    public CloudRoleTelemetryInitializer(string roleName)
    {
        _roleName = roleName;
        _roleInstance = DetermineRoleInstance();
    }

    /// <summary>
    /// Initializes telemetry with cloud role name and instance.
    /// </summary>
    /// <param name="telemetry">The telemetry to initialize.</param>
    public void Initialize(ITelemetry telemetry)
    {
        if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
        {
            telemetry.Context.Cloud.RoleName = _roleName;
        }

        if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleInstance))
        {
            telemetry.Context.Cloud.RoleInstance = _roleInstance;
        }
    }

    private static string DetermineRoleInstance()
    {
        // Check for Azure App Service environment
        var websiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        var websiteInstanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
        
        if (!string.IsNullOrEmpty(websiteName) && !string.IsNullOrEmpty(websiteInstanceId))
        {
            // Azure App Service: Use site name + instance ID
            return $"{websiteName}_{websiteInstanceId}";
        }

        // Check for Azure Container Instance or Docker
        var containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");
        var resourceGroup = Environment.GetEnvironmentVariable("RESOURCE_GROUP");
        
        if (!string.IsNullOrEmpty(containerName))
        {
            if (!string.IsNullOrEmpty(resourceGroup))
            {
                // Azure Container Instance: Use resource group + container name
                return $"{resourceGroup}_{containerName}";
            }
            // Docker without Azure: Use container name + machine name
            return $"{containerName}_{Environment.MachineName}";
        }

        // Check for Kubernetes
        var podName = Environment.GetEnvironmentVariable("HOSTNAME");
        var k8sNamespace = Environment.GetEnvironmentVariable("POD_NAMESPACE");
        
        if (!string.IsNullOrEmpty(podName) && !string.IsNullOrEmpty(k8sNamespace))
        {
            return $"{k8sNamespace}_{podName}";
        }

        // Default: On-premises or unrecognized environment - use machine name
        return Environment.MachineName;
    }
}
