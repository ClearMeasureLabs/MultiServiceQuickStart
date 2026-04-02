using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace ClearMeasure.HostedService.Infrastructure;

/// <summary>
/// Telemetry converter that sets the CloudRoleInstance property on all telemetry items.
/// </summary>
public class CloudRoleInstanceTelemetryConverter : TraceTelemetryConverter
{
    private readonly string _cloudRoleInstance;

    public CloudRoleInstanceTelemetryConverter(string cloudRoleInstance)
    {
        _cloudRoleInstance = cloudRoleInstance ?? throw new ArgumentNullException(nameof(cloudRoleInstance));
    }

    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        foreach (var telemetry in base.Convert(logEvent, formatProvider))
        {
            telemetry.Context.Cloud.RoleInstance = _cloudRoleInstance;
            yield return telemetry;
        }
    }
}
