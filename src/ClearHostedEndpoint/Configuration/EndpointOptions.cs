namespace ClearMeasure.HostedEndpoint.Configuration;

/// <summary>
/// Configuration options for NServiceBus endpoint hosting.
/// </summary>
public class EndpointOptions
{
    /// <summary>
    /// Gets or sets the endpoint name. If not set, the service type name will be used.
    /// </summary>
    public string? EndpointName { get; set; }

    /// <summary>
    /// Gets or sets whether to enable installers. Default is true for development convenience.
    /// </summary>
    public bool EnableInstallers { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to purge the queue on startup. Default is false.
    /// Should only be enabled in development/testing scenarios.
    /// </summary>
    public bool PurgeOnStartup { get; set; } = false;

    /// <summary>
    /// Gets or sets the error queue name. Default is "error".
    /// </summary>
    public string ErrorQueue { get; set; } = "error";

    /// <summary>
    /// Gets or sets the audit queue name. If null, auditing is disabled.
    /// </summary>
    public string? AuditQueue { get; set; }

    /// <summary>
    /// Gets or sets whether to enable metrics. Default is false.
    /// </summary>
    public bool EnableMetrics { get; set; } = false;

    /// <summary>
    /// Gets or sets the immediate retry count. Default is 3.
    /// </summary>
    public int ImmediateRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delayed retry count. Default is 3.
    /// </summary>
    public int DelayedRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delayed retry time increase. Default is 10 seconds.
    /// </summary>
    public TimeSpan DelayedRetryTimeIncrease { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the maximum concurrency for message processing. Default is 1.
    /// </summary>
    public int MaxConcurrency { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to enable outbox for exactly-once processing. Default is false.
    /// Requires SQL persistence to be configured.
    /// </summary>
    public bool EnableOutbox { get; set; } = false;

    /// <summary>
    /// Gets or sets the outbox cleanup batch size. Default is 100.
    /// </summary>
    public int OutboxCleanupBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets how long to keep outbox deduplication data. Default is 7 days.
    /// </summary>
    public TimeSpan OutboxTimeToKeepDeduplicationData { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets whether to enable timing behavior for NServiceBus handlers. Default is false.
    /// When enabled, handler execution times are logged with Application Insights metrics.
    /// </summary>
    public bool EnableTimingBehavior { get; set; } = false;
}
