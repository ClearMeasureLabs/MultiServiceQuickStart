using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace ClearMeasure.HostedService.Infrastructure;

/// <summary>
/// Telemetry converter that sets the CloudRoleName property on all telemetry items.
/// </summary>
public class CloudRoleNameTelemetryConverter : TraceTelemetryConverter
{
    private readonly string _cloudRoleName;

    public CloudRoleNameTelemetryConverter(string cloudRoleName)
    {
        _cloudRoleName = cloudRoleName ?? throw new ArgumentNullException(nameof(cloudRoleName));
    }

    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        foreach (var telemetry in base.Convert(logEvent, formatProvider))
        {
            telemetry.Context.Cloud.RoleName = _cloudRoleName;
            yield return telemetry;
        }
    }
}
