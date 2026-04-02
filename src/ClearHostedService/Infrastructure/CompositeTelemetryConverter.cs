using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace ClearMeasure.HostedService.Infrastructure;

/// <summary>
/// Telemetry converter that sets both CloudRoleName and CloudRoleInstance properties on all telemetry items.
/// </summary>
public class CompositeTelemetryConverter : TraceTelemetryConverter
{
    private readonly string _cloudRoleName;
    private readonly string _cloudRoleInstance;

    public CompositeTelemetryConverter(string cloudRoleName, string cloudRoleInstance)
    {
        _cloudRoleName = cloudRoleName ?? throw new ArgumentNullException(nameof(cloudRoleName));
        _cloudRoleInstance = cloudRoleInstance ?? throw new ArgumentNullException(nameof(cloudRoleInstance));
    }

    public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
    {
        foreach (var telemetry in base.Convert(logEvent, formatProvider))
        {
            telemetry.Context.Cloud.RoleName = _cloudRoleName;
            telemetry.Context.Cloud.RoleInstance = _cloudRoleInstance;
            yield return telemetry;
        }
    }
}
