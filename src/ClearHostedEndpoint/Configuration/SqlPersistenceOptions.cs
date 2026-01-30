namespace ClearMeasure.HostedEndpoint.Configuration;

/// <summary>
/// Configuration options for SQL persistence.
/// </summary>
public class SqlPersistenceOptions
{
    /// <summary>
    /// Gets or sets the connection string for SQL persistence.
    /// This is required when using SQL persistence for sagas.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the schema name for SQL persistence tables. Default is "dbo".
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the table prefix for SQL persistence tables.
    /// If null, the endpoint name will be used.
    /// </summary>
    public string? TablePrefix { get; set; }

    /// <summary>
    /// Gets or sets whether to enable saga persistence. Default is true.
    /// </summary>
    public bool EnableSagaPersistence { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable subscription storage. Default is false.
    /// Only needed for pub/sub with transports that don't have native pub/sub.
    /// </summary>
    public bool EnableSubscriptionStorage { get; set; } = false;

    /// <summary>
    /// Gets or sets the subscription cache period. Default is 5 seconds.
    /// </summary>
    public TimeSpan SubscriptionCachePeriod { get; set; } = TimeSpan.FromSeconds(5);
}
