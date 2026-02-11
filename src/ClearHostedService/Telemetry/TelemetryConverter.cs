using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace ClearMeasure.HostedService.Telemetry;

/// <summary>
/// Custom telemetry converter that sets CloudRoleName and CloudRoleInstance properties.
/// </summary>
public class TelemetryConverter : TraceTelemetryConverter
{
    private readonly string _applicationName;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryConverter"/> class.
    /// </summary>
    /// <param name="applicationName">The application name to use for CloudRoleName.</param>
    public TelemetryConverter(string applicationName)
    {
        _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
    }

    /// <summary>
    /// Converts a Serilog log event to Application Insights telemetry.
    /// </summary>
    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        var telemetries = base.Convert(logEvent, formatProvider);

        foreach (var telemetry in telemetries)
        {
            if (telemetry is ISupportProperties supportProperties)
            {
                telemetry.Context.Cloud.RoleName = _applicationName;
                telemetry.Context.Cloud.RoleInstance = GetCloudRoleInstance();
            }
        }

        return telemetries;
    }

    /// <summary>
    /// Gets the CloudRoleInstance value based on the environment.
    /// </summary>
    /// <returns>
    /// Machine name for on-premises environments, 
    /// or ResourceGroup+Instance name for Docker/Azure environments.
    /// </returns>
    private static string GetCloudRoleInstance()
    {
        var websiteSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        var websiteInstanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
        var resourceGroup = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP");

        if (!string.IsNullOrEmpty(websiteSiteName))
        {
            if (!string.IsNullOrEmpty(resourceGroup))
            {
                return $"{resourceGroup}/{websiteSiteName}";
            }

            if (!string.IsNullOrEmpty(websiteInstanceId))
            {
                return $"{websiteSiteName}/{websiteInstanceId}";
            }

            return websiteSiteName;
        }

        var containerName = Environment.GetEnvironmentVariable("HOSTNAME");
        if (!string.IsNullOrEmpty(containerName) && containerName.Length > 12)
        {
            return containerName;
        }

        return Environment.MachineName;
    }
}
