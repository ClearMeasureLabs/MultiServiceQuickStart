using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using ClearMeasure.HostedEndpoint.Exceptions;
using ClearMeasure.HostedEndpoint.Infrastructure.Behaviors;
using ClearMeasure.HostedService;
using Microsoft.Data.SqlClient;
using ClearMeasure.HostedEndpoint.Configuration;
using Microsoft.Extensions.Configuration;

namespace ClearMeasure.HostedEndpoint;

/// <summary>
/// Base class for hosting NServiceBus endpoints as a hosted service.
/// Provides reasonable defaults for endpoint configuration with SQL persistence for sagas.
/// </summary>
public abstract class ClearHostedEndpoint : ClearHostedService
{
    private IEndpointInstance? _endpointInstance;
    private IServiceCollection? _nsbServiceCollection;

    protected ClearHostedEndpoint(IConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    /// Gets the NServiceBus endpoint instance. Only available after StartAsync has completed.
    /// </summary>
    protected IEndpointInstance EndpointInstance
    {
        get
        {
            if (_endpointInstance == null)
            {
                throw new InvalidOperationException(
                    "EndpointInstance is not available. This should only be accessed after StartAsync has been called.");
            }
            return _endpointInstance;
        }
    }

    /// <summary>
    /// Gets the endpoint options.
    /// </summary>
    protected virtual EndpointOptions EndpointOptions { get; } = new();

    /// <summary>
    /// Gets the SQL persistence options. Return null to disable SQL persistence.
    /// </summary>
    protected virtual SqlPersistenceOptions? SqlPersistenceOptions { get; }

    /// <summary>
    /// Gets the effective endpoint name.
    /// </summary>
    protected string EffectiveEndpointName => EndpointOptions.EndpointName ?? GetType().Name;

    /// <summary>
    /// Creates the service collection. NServiceBus will use RegisterComponents to add its services.
    /// </summary>
    protected override IServiceCollection CreateServiceCollection()
    {
        _nsbServiceCollection = new ServiceCollection();
        return _nsbServiceCollection;
    }

    /// <summary>
    /// Builds the service provider by starting the NServiceBus endpoint.
    /// The endpoint startup process creates and configures the service provider.
    /// </summary>
    protected override async Task<IServiceProvider> BuildServiceProviderAsync(IServiceCollection services, CancellationToken cancellationToken)
    {
        try
        {
            var endpointConfiguration = CreateEndpointConfiguration();

            ConfigureEndpoint(endpointConfiguration);
            ConfigureTransport(endpointConfiguration);
            ConfigureSerialization(endpointConfiguration);
            ConfigurePersistence(endpointConfiguration);
            ConfigureRecoverability(endpointConfiguration);

            // Register user dependencies through NServiceBus's RegisterComponents
            // In NServiceBus 9, use the IServiceCollection extension method
            endpointConfiguration.RegisterComponents(configureServices =>
            {
                foreach (var descriptor in services)
                {
                    configureServices.Add(descriptor);
                }
            });

            // Allow derived classes to do final configuration
            await ConfigureEndpointAsync(endpointConfiguration, cancellationToken);

            Logger.Information("Starting NServiceBus endpoint: {EndpointName}", EffectiveEndpointName);

            _endpointInstance = await Endpoint.Start(endpointConfiguration, cancellationToken);

            Logger.Information("NServiceBus endpoint started successfully: {EndpointName}", EffectiveEndpointName);

            // Return a minimal service provider since NServiceBus manages its own DI
            return services.BuildServiceProvider();
        }
        catch (Exception ex)
        {
            throw new EndpointConfigurationException(
                $"Failed to start NServiceBus endpoint '{EffectiveEndpointName}'. See inner exception for details.", ex);
        }
    }

    /// <summary>
    /// Creates the base endpoint configuration with common settings.
    /// </summary>
    private EndpointConfiguration CreateEndpointConfiguration()
    {
        var endpointConfiguration = new EndpointConfiguration(EffectiveEndpointName);

        // Enable installers if configured
        if (EndpointOptions.EnableInstallers)
        {
            endpointConfiguration.EnableInstallers();
        }

        // Configure purge on startup (development only)
        if (EndpointOptions.PurgeOnStartup)
        {
            Logger.Warning("PurgeOnStartup is enabled for endpoint {EndpointName}. This should only be used in development.", EffectiveEndpointName);
            endpointConfiguration.PurgeOnStartup(true);
        }

        // Configure error queue
        endpointConfiguration.SendFailedMessagesTo(EndpointOptions.ErrorQueue);

        // Configure audit if specified
        if (!string.IsNullOrEmpty(EndpointOptions.AuditQueue))
        {
            endpointConfiguration.AuditProcessedMessagesTo(EndpointOptions.AuditQueue);
        }

        // Configure concurrency
        endpointConfiguration.LimitMessageProcessingConcurrencyTo(EndpointOptions.MaxConcurrency);

        // Configure timing behavior if enabled
        if (EndpointOptions.EnableTimingBehavior)
        {
            var pipeline = endpointConfiguration.Pipeline;
            pipeline.Register(typeof(TimingBehavior), "Logs handler execution times for Application Insights metrics");
        }

        return endpointConfiguration;
    }

    /// <summary>
    /// Configures the message transport. This method must be overridden to specify the transport.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    protected abstract void ConfigureTransport(EndpointConfiguration endpointConfiguration);

    /// <summary>
    /// Override this method to perform additional endpoint configuration.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    protected virtual void ConfigureEndpoint(EndpointConfiguration endpointConfiguration)
    {
        // Default implementation - no additional configuration
    }

    /// <summary>
    /// Configures the message serializer. Override to use a different serializer.
    /// Default implementation uses System.Text.Json serialization.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    protected virtual void ConfigureSerialization(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    }

    /// <summary>
    /// Override this method to perform additional async endpoint configuration.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    protected virtual Task ConfigureEndpointAsync(EndpointConfiguration endpointConfiguration, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Configures SQL persistence for sagas if SqlPersistenceOptions is provided.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    protected virtual void ConfigurePersistence(EndpointConfiguration endpointConfiguration)
    {
        var sqlOptions = SqlPersistenceOptions;
        if (sqlOptions == null)
        {
            // Use in-memory learning persistence when no SQL options provided
            Logger.Warning("No SQL persistence configured for endpoint {EndpointName}. Using LearningPersistence (not for production).", EffectiveEndpointName);
            endpointConfiguration.UsePersistence<LearningPersistence>();
            return;
        }

        if (string.IsNullOrEmpty(sqlOptions.ConnectionString))
        {
            throw new EndpointConfigurationException(
                "SqlPersistenceOptions.ConnectionString is required when SQL persistence is configured.");
        }

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();

        // Configure SQL dialect for SQL Server
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        dialect.Schema(sqlOptions.Schema);

        // Configure connection builder
        persistence.ConnectionBuilder(() => CreateDbConnection(sqlOptions.ConnectionString));

        // Configure table prefix
        var tablePrefix = sqlOptions.TablePrefix ?? EffectiveEndpointName.Replace(".", "_");
        persistence.TablePrefix(tablePrefix);

        // Configure saga persistence
        if (sqlOptions.EnableSagaPersistence)
        {
            var sagaSettings = persistence.SagaSettings();
            // Configure saga serializer to use JSON
            sagaSettings.JsonSettings(new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            });
        }

        // Configure subscription storage
        if (sqlOptions.EnableSubscriptionStorage)
        {
            var subscriptionSettings = persistence.SubscriptionSettings();
            subscriptionSettings.CacheFor(sqlOptions.SubscriptionCachePeriod);
        }

        // Configure outbox if enabled
        if (EndpointOptions.EnableOutbox)
        {
            var outbox = endpointConfiguration.EnableOutbox();
            outbox.KeepDeduplicationDataFor(EndpointOptions.OutboxTimeToKeepDeduplicationData);
        }

        Logger.Information("SQL persistence configured for endpoint {EndpointName} with schema {Schema}",
            EffectiveEndpointName, sqlOptions.Schema);
    }

    /// <summary>
    /// Creates a database connection for SQL persistence.
    /// Override this method to provide a custom connection type (e.g., for different database providers).
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>A database connection.</returns>
    protected virtual DbConnection CreateDbConnection(string connectionString)
    {
        // Default implementation uses SQL Server
        return new SqlConnection(connectionString);
    }

    /// <summary>
    /// Configures the recoverability (retry) policy.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    protected virtual void ConfigureRecoverability(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.Recoverability()
            .Immediate(immediate => immediate.NumberOfRetries(EndpointOptions.ImmediateRetryCount))
            .Delayed(delayed => delayed
                .NumberOfRetries(EndpointOptions.DelayedRetryCount)
                .TimeIncrease(EndpointOptions.DelayedRetryTimeIncrease));
    }

    /// <summary>
    /// The main execution loop. For NServiceBus endpoints, this keeps the service running
    /// until cancellation is requested. Override to add periodic tasks.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // NServiceBus handles message processing internally.
        // Keep the service alive until stop is requested.
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }

    /// <summary>
    /// Called when the hosted service is stopping. Stops the NServiceBus endpoint.
    /// </summary>
    public override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        if (_endpointInstance != null)
        {
            Logger.Information("Stopping NServiceBus endpoint: {EndpointName}", EffectiveEndpointName);

            try
            {
                await _endpointInstance.Stop(cancellationToken);
                Logger.Information("NServiceBus endpoint stopped: {EndpointName}", EffectiveEndpointName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error stopping NServiceBus endpoint: {EndpointName}", EffectiveEndpointName);
                throw;
            }
        }

        await base.OnStoppingAsync(cancellationToken);
    }

    /// <summary>
    /// Disposes of resources used by the endpoint service.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Endpoint instance doesn't implement IDisposable, but clean up reference
            _endpointInstance = null;
        }

        base.Dispose(disposing);
    }
}
