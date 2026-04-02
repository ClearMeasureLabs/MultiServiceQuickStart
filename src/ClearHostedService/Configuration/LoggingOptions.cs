using Serilog;
using Serilog.Events;

namespace ClearMeasure.HostedService.Configuration;

/// <summary>
/// Configuration options for logging.
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Gets or sets the directory where log files will be written.
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// Gets or sets the rolling interval for log files.
    /// </summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Gets or sets a value indicating whether console logging is enabled.
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether file logging is enabled.
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether ApplicationInsights is enabled.
    /// </summary>
    public bool EnableApplicationInsights { get; set; } = false;

    /// <summary>
    /// Gets or sets the ApplicationInsights connection string.
    /// </summary>
    public string? ApplicationInsightsConnectionString { get; set; }
    
    /// <summary>
    /// Gets or sets the application name (CloudRoleName) for ApplicationInsights telemetry.
    /// Defaults to the service type name if not specified.
    /// </summary>
    public string? ApplicationName { get; set; }
    
    /// <summary>
    /// Gets or sets the cloud instance name (CloudRoleInstance) for ApplicationInsights telemetry.
    /// Defaults to machine name for on-prem or resource group + instance name for cloud deployments.
    /// </summary>
    public string? CloudInstanceName { get; set; }

    /// <summary>
    /// Gets or sets the output template for log messages.
    /// </summary>
    public string OutputTemplate { get; set; } = 
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
}
