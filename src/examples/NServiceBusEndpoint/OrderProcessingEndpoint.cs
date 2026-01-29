using NServiceBus;
using QuickHostedEndpoint.Application;
using QuickHostedEndpoint.Infrastructure.Configuration;

namespace NServiceBusEndpoint;

/// <summary>
/// An example NServiceBus endpoint that processes order-related messages.
/// Demonstrates usage of QuickHostedEndpoint with Learning Transport.
/// </summary>
public class OrderProcessingEndpoint : QuickHostedEndpoint.Application.QuickHostedEndpoint
{
    /// <summary>
    /// Configure endpoint options for the order processing endpoint.
    /// </summary>
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "OrderProcessing",
        EnableInstallers = true,
        MaxConcurrency = 1,
        ImmediateRetryCount = 2,
        DelayedRetryCount = 2,
        DelayedRetryTimeIncrease = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    /// Configure the Learning Transport for development/demo purposes.
    /// In production, you would use a real transport like Azure Service Bus, RabbitMQ, etc.
    /// </summary>
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // Use Learning Transport for this example (file-based, development only)
        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        
        // Configure the storage directory for messages
        var learningTransportDirectory = Path.Combine(
            Path.GetTempPath(),
            "NServiceBusEndpointExample",
            ".learningtransport");
        
        transport.StorageDirectory(learningTransportDirectory);
        
        Logger.Information("Learning Transport configured at: {Directory}", learningTransportDirectory);
    }

    /// <summary>
    /// Additional endpoint configuration.
    /// </summary>
    protected override void ConfigureEndpoint(EndpointConfiguration endpointConfiguration)
    {
        // Scan only this assembly for message handlers
        endpointConfiguration.AssemblyScanner()
            .ExcludeAssemblies("Microsoft", "System", "Serilog", "NServiceBus.Persistence");
    }

    /// <summary>
    /// Called when the endpoint is starting.
    /// </summary>
    public override async Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Order Processing Endpoint is initializing...");
        
        // Simulate some initialization work
        await Task.Delay(100, cancellationToken);
        
        Logger.Information("Order Processing Endpoint initialization complete.");
    }

    /// <summary>
    /// Called when the endpoint is stopping.
    /// </summary>
    public override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Order Processing Endpoint is shutting down...");
        
        // Call base to stop the NServiceBus endpoint
        await base.OnStoppingAsync(cancellationToken);
        
        Logger.Information("Order Processing Endpoint shutdown complete.");
    }
}
