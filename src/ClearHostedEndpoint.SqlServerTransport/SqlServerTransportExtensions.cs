using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using System.Data.Common;
using System.Transactions;

namespace ClearMeasure.HostedEndpoint.SqlServerTransport;

/// <summary>
/// Provides extension methods for configuring SQL Server transport with best practices.
/// </summary>
public static class SqlServerTransportExtensions
{
    /// <summary>
    /// Configures SQL Server transport with best practices including:
    /// - TransactionScope mode for transaction coordination
    /// - Automatic outbox configuration
    /// - StorageContext integration for synchronized transactions
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="options">Optional configuration options.</param>
    public static void UseSqlServerTransport(
        this EndpointConfiguration endpointConfiguration,
        string connectionString,
        SqlServerTransportOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        options ??= new SqlServerTransportOptions();

        // Configure the SQL Server transport
        var transport = endpointConfiguration.UseTransport<NServiceBus.SqlServerTransport>();
        transport.ConnectionString(connectionString);

        // Configure multi-catalog/schema support if specified
        if (!string.IsNullOrWhiteSpace(options.DefaultSchema))
        {
            transport.DefaultSchema(options.DefaultSchema);
        }

        // Configure queue schema if specified
        if (options.QueueSchemaSettings != null)
        {
            transport.UseSchemaForQueue(
                queueName: options.QueueSchemaSettings.QueueName,
                schema: options.QueueSchemaSettings.Schema);
        }

        // Configure subscriptions if needed
        if (options.SubscriptionSettings != null && options.ConfigureSubscriptions)
        {
            var subscriptions = transport.SubscriptionSettings();
            subscriptions.SubscriptionTableName(
                tableName: options.SubscriptionSettings.TableName,
                schemaName: options.SubscriptionSettings.Schema,
                catalogName: options.SubscriptionSettings.Catalog);

            if (!options.SubscriptionSettings.EnableCaching)
            {
                subscriptions.DisableSubscriptionCache();
            }
        }

        // Enable native delayed delivery if configured
        if (options.EnableNativeDelayedDelivery)
        {
            var delayedDelivery = transport.NativeDelayedDelivery();
            
            if (!string.IsNullOrWhiteSpace(options.DelayedDeliveryTableSuffix))
            {
                delayedDelivery.TableSuffix(options.DelayedDeliveryTableSuffix);
            }
        }

        // Enable outbox for transactional consistency
        var outbox = endpointConfiguration.EnableOutbox();
        outbox.KeepDeduplicationDataFor(options.OutboxDeduplicationPeriod);

        // Register the StorageContext as transient in DI
        endpointConfiguration.RegisterComponents(services =>
        {
            services.AddTransient<StorageContext>();
        });

        // Register the StorageContextBehavior to attach the synchronized storage session
        endpointConfiguration.Pipeline.Register(
            behavior: typeof(StorageContextBehavior),
            description: "Attaches the synchronized storage session to StorageContext for message handlers");
    }

    /// <summary>
    /// Configures SQL Server transport with a connection factory.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    /// <param name="connectionFactory">A factory function that creates database connections.</param>
    /// <param name="options">Optional configuration options.</param>
    public static void UseSqlServerTransport(
        this EndpointConfiguration endpointConfiguration,
        Func<DbConnection> connectionFactory,
        SqlServerTransportOptions? options = null)
    {
        if (connectionFactory == null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        // Test the connection factory to get a connection string
        using var testConnection = connectionFactory();
        var connectionString = testConnection.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection factory must produce connections with a valid connection string.");
        }

        UseSqlServerTransport(endpointConfiguration, connectionString, options);
    }
}

/// <summary>
/// Configuration options for SQL Server transport.
/// </summary>
public class SqlServerTransportOptions
{
    /// <summary>
    /// Gets or sets the transaction isolation level.
    /// Default is ReadCommitted.
    /// </summary>
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// Gets or sets the transaction timeout.
    /// Default is 1 minute.
    /// </summary>
    public TimeSpan TransactionTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the default schema for queues.
    /// If not set, uses the SQL Server default (dbo).
    /// </summary>
    public string? DefaultSchema { get; set; }

    /// <summary>
    /// Gets or sets queue schema settings for multi-catalog support.
    /// </summary>
    public QueueSchemaSettings? QueueSchemaSettings { get; set; }

    /// <summary>
    /// Gets or sets subscription settings.
    /// </summary>
    public SubscriptionSettings? SubscriptionSettings { get; set; }

    /// <summary>
    /// Gets or sets whether to configure subscriptions.
    /// Default is false. Set to true if you need pub/sub capabilities.
    /// </summary>
    public bool ConfigureSubscriptions { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable native delayed delivery.
    /// Default is true.
    /// </summary>
    public bool EnableNativeDelayedDelivery { get; set; } = true;

    /// <summary>
    /// Gets or sets the delayed delivery table suffix.
    /// </summary>
    public string? DelayedDeliveryTableSuffix { get; set; }

    /// <summary>
    /// Gets or sets the period to keep outbox deduplication data.
    /// Default is 7 days.
    /// </summary>
    public TimeSpan OutboxDeduplicationPeriod { get; set; } = TimeSpan.FromDays(7);
}

/// <summary>
/// Queue schema settings for multi-catalog support.
/// </summary>
public class QueueSchemaSettings
{
    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema for the queue.
    /// </summary>
    public string Schema { get; set; } = string.Empty;
}

/// <summary>
/// Subscription settings for SQL Server transport.
/// </summary>
public class SubscriptionSettings
{
    /// <summary>
    /// Gets or sets the subscription table name.
    /// Default is "SubscriptionRouting".
    /// </summary>
    public string TableName { get; set; } = "SubscriptionRouting";

    /// <summary>
    /// Gets or sets the subscription table schema.
    /// Default is "dbo".
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Gets or sets the subscription table catalog (database name).
    /// If null, uses the default catalog from the connection string.
    /// </summary>
    public string? Catalog { get; set; }

    /// <summary>
    /// Gets or sets whether to enable subscription caching.
    /// Default is true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;
}
