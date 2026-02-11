using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ClearMeasure.HostedService.Infrastructure.TelemetryConverters;

/// <summary>
/// Telemetry converter that sets the CloudRoleInstance property on Application Insights telemetry.
/// </summary>
public class CloudInstanceConverter : ITelemetryInitializer
{
    private readonly string _cloudInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudInstanceConverter"/> class.
    /// </summary>
    /// <param name="cloudInstance">The cloud instance identifier to use for telemetry.</param>
    public CloudInstanceConverter(string cloudInstance)
    {
        _cloudInstance = cloudInstance ?? throw new ArgumentNullException(nameof(cloudInstance));
    }

    /// <summary>
    /// Sets the CloudRoleInstance on the telemetry item.
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
