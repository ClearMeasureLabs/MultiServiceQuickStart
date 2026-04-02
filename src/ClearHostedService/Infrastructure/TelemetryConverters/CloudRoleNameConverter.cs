using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ClearMeasure.HostedService.Infrastructure.TelemetryConverters;

/// <summary>
/// Telemetry converter that sets the CloudRoleName property on Application Insights telemetry.
/// </summary>
public class CloudRoleNameConverter : ITelemetryInitializer
{
    private readonly string _roleName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudRoleNameConverter"/> class.
    /// </summary>
    /// <param name="roleName">The cloud role name to use for telemetry.</param>
    public CloudRoleNameConverter(string roleName)
    {
        _roleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
    }

    /// <summary>
    /// Sets the CloudRoleName on the telemetry item.
    /// </summary>
    /// <param name="telemetry">The telemetry item to initialize.</param>
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry == null)
        {
            return;
        }

        telemetry.Context.Cloud.RoleName = _roleName;
    }
}
