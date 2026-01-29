# QuickHostedEndpoint Architecture

This document describes the architecture and design of the QuickHostedEndpoint library.

## Overview

QuickHostedEndpoint extends `BaseHostedService` from QuickHostedService to provide NServiceBus endpoint hosting capabilities. It inherits all the benefits of the base library while adding messaging-specific functionality.

## Inheritance Hierarchy

```
IHostedService (Microsoft)
    ??? BaseHostedService (QuickHostedService)
            ??? BaseEndpointService (QuickHostedEndpoint)
                    ??? YourEndpoint (Your Application)
```

## Architecture Diagram

```
???????????????????????????????????????????????????????????????????
?                     Your Application                             ?
???????????????????????????????????????????????????????????????????
?  OrderProcessingEndpoint : BaseEndpointService                  ?
?  ??? ConfigureTransport()      - Define messaging transport     ?
?  ??? ConfigureEndpoint()       - Additional endpoint config     ?
?  ??? Message Handlers          - Business logic                 ?
???????????????????????????????????????????????????????????????????
?                   QuickHostedEndpoint                           ?
???????????????????????????????????????????????????????????????????
?  BaseEndpointService                                            ?
?  ??? CreateEndpointConfiguration()  - Base NServiceBus setup   ?
?  ??? ConfigurePersistence()         - SQL persistence          ?
?  ??? ConfigureRecoverability()      - Retry policies           ?
?  ??? ConfigureSerialization()       - JSON serialization       ?
?  ??? BuildServiceProviderAsync()    - Start endpoint           ?
???????????????????????????????????????????????????????????????????
?                    QuickHostedService                           ?
???????????????????????????????????????????????????????????????????
?  BaseHostedService                                              ?
?  ??? StartAsync() / StopAsync()     - Lifecycle management     ?
?  ??? Logger                         - Serilog integration      ?
?  ??? ServiceProvider                - Isolated DI container    ?
?  ??? OnStartingAsync/OnStoppingAsync - Lifecycle hooks         ?
???????????????????????????????????????????????????????????????????
?                    .NET Hosting                                 ?
?  IHostedService, IServiceCollection, IHost                      ?
???????????????????????????????????????????????????????????????????
```

## Component Responsibilities

### BaseEndpointService

The core class that bridges QuickHostedService with NServiceBus:

| Method | Responsibility |
|--------|----------------|
| `CreateEndpointConfiguration()` | Creates NServiceBus configuration with common settings |
| `ConfigureTransport()` | **Abstract** - Must be implemented to specify transport |
| `ConfigureEndpoint()` | Optional additional endpoint configuration |
| `ConfigureEndpointAsync()` | Optional async endpoint configuration |
| `ConfigureSerialization()` | Sets up JSON serialization (overridable) |
| `ConfigurePersistence()` | Configures SQL persistence or LearningPersistence |
| `ConfigureRecoverability()` | Sets up retry policies |
| `BuildServiceProviderAsync()` | Starts the endpoint and returns service provider |
| `OnStoppingAsync()` | Gracefully stops the NServiceBus endpoint |

### EndpointOptions

Configuration class for endpoint behavior:

- Endpoint naming and queue configuration
- Concurrency and retry settings
- Outbox configuration for exactly-once processing

### SqlPersistenceOptions

Configuration class for SQL Server persistence:

- Connection string and schema settings
- Saga and subscription storage configuration
- Table prefix customization

## Lifecycle Flow

```
Host.StartAsync()
    ?
    ?
BaseHostedService.StartAsync()
    ?
    ??? CreateServiceCollection()
    ??? RegisterDependencyInjection()
    ??? ConfigureLogging()
    ?
    ?
BaseEndpointService.BuildServiceProviderAsync()
    ?
    ??? CreateEndpointConfiguration()
    ?   ??? Set endpoint name
    ?   ??? Enable installers (if configured)
    ?   ??? Configure error/audit queues
    ?   ??? Set concurrency limit
    ?
    ??? ConfigureEndpoint()           ? Override for custom config
    ??? ConfigureTransport()          ? MUST override (abstract)
    ??? ConfigureSerialization()      ? Override for custom serializer
    ??? ConfigurePersistence()        ? Override for custom persistence
    ??? ConfigureRecoverability()     ? Override for custom retry policy
    ??? RegisterComponents()          ? Inject DI services
    ??? ConfigureEndpointAsync()      ? Override for async config
    ?
    ?
Endpoint.Start()                      ? NServiceBus starts
    ?
    ?
OnStartingAsync()                     ? Lifecycle hook
    ?
    ?
ExecuteAsync()                        ? Waits for cancellation
    ?
    ? (on shutdown)
OnStoppingAsync()
    ?
    ??? EndpointInstance.Stop()       ? Graceful NServiceBus shutdown
    ?
    ?
Dispose()
```

## Transport Configuration Pattern

The `ConfigureTransport()` method is abstract, requiring derived classes to specify their transport:

```csharp
// RabbitMQ Example
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
    transport.ConnectionString("host=rabbitmq;username=guest;password=guest");
    
    var routing = transport.Routing();
    routing.RouteToEndpoint(typeof(ProcessPayment), "PaymentService");
}

// Azure Service Bus Example
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
    transport.ConnectionString(Environment.GetEnvironmentVariable("ASB_CONNECTION"));
}

// Learning Transport (Development)
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var transport = endpointConfiguration.UseTransport<LearningTransport>();
    transport.StorageDirectory(Path.Combine(Path.GetTempPath(), "learning-transport"));
}
```

## Persistence Strategy

The library supports two persistence modes:

### 1. LearningPersistence (Default - Development Only)
When `SqlPersistenceOptions` is null, the library uses in-memory learning persistence:
```csharp
// No SqlPersistenceOptions = LearningPersistence
protected override SqlPersistenceOptions? SqlPersistenceOptions => null;
```

### 2. SQL Persistence (Production)
When `SqlPersistenceOptions` is provided:
```csharp
protected override SqlPersistenceOptions? SqlPersistenceOptions => new()
{
    ConnectionString = "Server=.;Database=Sagas;Integrated Security=true",
    Schema = "nsb",
    EnableSagaPersistence = true,
    EnableSubscriptionStorage = true
};
```

## Error Handling

The library wraps NServiceBus startup failures in `EndpointConfigurationException`:

```csharp
try
{
    _endpointInstance = await Endpoint.Start(endpointConfiguration, cancellationToken);
}
catch (Exception ex)
{
    throw new EndpointConfigurationException(
        $"Failed to start NServiceBus endpoint '{EffectiveEndpointName}'.", ex);
}
```

## Design Decisions

1. **Abstract Transport Configuration**: Forces explicit transport choice rather than providing a default that might not be suitable.

2. **SQL Server Default**: For SQL persistence, defaults to SQL Server via `Microsoft.Data.SqlClient`, but `CreateDbConnection()` can be overridden for other databases.

3. **JSON Serialization**: Uses `SystemJsonSerializer` by default for modern .NET compatibility.

4. **Isolated Service Provider**: Each endpoint has its own DI container, inherited from BaseHostedService.

5. **Graceful Shutdown**: Properly stops NServiceBus endpoint in `OnStoppingAsync()` before disposal.
