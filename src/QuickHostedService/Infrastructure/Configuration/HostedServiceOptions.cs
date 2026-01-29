namespace QuickHostedService.Infrastructure.Configuration;

/// <summary>
/// Configuration options for hosted services.
/// </summary>
public class HostedServiceOptions
{
    /// <summary>
    /// Gets or sets the timeout for graceful shutdown.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a value indicating whether detailed error information should be logged.
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets the service name for logging and identification.
    /// </summary>
    public string? ServiceName { get; set; }
}
