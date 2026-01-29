# NServiceBus Endpoint Example

This example demonstrates how to use the `QuickHostedEndpoint` library to host an NServiceBus 9 endpoint as a hosted service.

## Overview

The example creates an order processing endpoint that:
- Uses the **Learning Transport** (file-based, for development only)
- Handles `PlaceOrder` commands
- Publishes `OrderPlaced` events
- Demonstrates message handler patterns

## Project Structure

```
NServiceBusEndpoint/
??? Program.cs                    # Host configuration
??? OrderProcessingEndpoint.cs    # The endpoint service (extends BaseEndpointService)
??? Messages/
?   ??? PlaceOrder.cs            # Command message
?   ??? OrderPlaced.cs           # Event message
??? Handlers/
    ??? PlaceOrderHandler.cs     # Handles PlaceOrder commands
    ??? OrderPlacedHandler.cs    # Handles OrderPlaced events
```

## Running the Example

```bash
dotnet run --project examples/NServiceBusEndpoint/NServiceBusEndpoint.csproj
```

The endpoint will start and wait for messages. Since this uses the Learning Transport, messages are stored in files at:
`%TEMP%\NServiceBusEndpointExample\.learningtransport`

## Key Concepts

### BaseEndpointService

The `OrderProcessingEndpoint` class extends `BaseEndpointService` and demonstrates:

1. **Endpoint Options**: Configure endpoint name, concurrency, retries
2. **Transport Configuration**: Abstract method that must be implemented
3. **Lifecycle Hooks**: `OnStartingAsync` and `OnStoppingAsync` for initialization/cleanup

### Transport Configuration

The `ConfigureTransport` method is **abstract** in `BaseEndpointService`. You must implement it to specify which transport to use:

```csharp
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var transport = endpointConfiguration.UseTransport<LearningTransport>();
    transport.StorageDirectory(learningTransportDirectory);
}
```

For production, you would use a real transport:
- Azure Service Bus: `NServiceBus.Transport.AzureServiceBus`
- RabbitMQ: `NServiceBus.RabbitMQ`
- SQL Server: `NServiceBus.Transport.SqlServer`

### SQL Persistence (Optional)

This example doesn't use SQL persistence (no sagas). To enable it, override `SqlPersistenceOptions`:

```csharp
protected override SqlPersistenceOptions? SqlPersistenceOptions { get; } = new()
{
    ConnectionString = "your-connection-string",
    Schema = "dbo",
    EnableSagaPersistence = true
};
```

## Testing Messages

To send a test message, you can create a simple sender or use the NServiceBus management tools. The Learning Transport stores messages as files, so you can also manually create message files for testing.

## Production Considerations

When moving to production:

1. **Replace Learning Transport** with a production transport (Azure Service Bus, RabbitMQ, etc.)
2. **Configure SQL Persistence** for sagas if needed
3. **Set appropriate concurrency** based on your workload
4. **Configure proper error handling** and monitoring
5. **Disable installers** (`EnableInstallers = false`) and manage infrastructure separately
