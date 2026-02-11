using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ClearMeasure.HostedService.Infrastructure.TelemetryConverters;

/// <summary>
/// Telemetry initializer that sets the CloudRoleInstance property.
/// Uses either the provided cloud instance identifier or defaults to the machine name.
/// </summary>
public class CloudInstanceConverter : ITelemetryInitializer
{
    private readonly string _cloudInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudInstanceConverter"/> class.
    /// </summary>
    /// <param name="cloudInstance">The cloud instance identifier (machine name or resource group + instance). If null, uses Environment.MachineName.</param>
    public CloudInstanceConverter(string? cloudInstance = null)
    {
        _cloudInstance = string.IsNullOrWhiteSpace(cloudInstance) 
            ? Environment.MachineName 
            : cloudInstance;
    }

    /// <summary>
    /// Initializes the telemetry item by setting the CloudRoleInstance property.
    /// </summary>
    /// <param name="telemetry">The telemetry item to initialize.</param>
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry == null)
        {
            return;
        }

        telemetry.Context.Cloud.RoleInstance = _cloudInstance;
    }
}
