using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ClearMeasure.HostedService.Infrastructure.TelemetryConverters;

/// <summary>
/// Telemetry initializer that sets the CloudRoleName property from the application name.
/// </summary>
public class CloudRoleNameConverter : ITelemetryInitializer
{
    private readonly string _applicationName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudRoleNameConverter"/> class.
    /// </summary>
    /// <param name="applicationName">The application name to use for CloudRoleName.</param>
    /// <exception cref="ArgumentNullException">Thrown when applicationName is null or empty.</exception>
    public CloudRoleNameConverter(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentNullException(nameof(applicationName), "Application name cannot be null or empty.");
        }

        _applicationName = applicationName;
    }

    /// <summary>
    /// Initializes the telemetry item by setting the CloudRoleName property.
    /// </summary>
    /// <param name="telemetry">The telemetry item to initialize.</param>
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry == null)
        {
            return;
        }

        telemetry.Context.Cloud.RoleName = _applicationName;
    }
}
