# QuickHostedEndpoint

An extension of QuickHostedService that provides a streamlined base implementation for hosting NServiceBus endpoints as background services in .NET.

## Overview

QuickHostedEndpoint builds on top of `BaseHostedService` to provide an opinionated foundation for creating NServiceBus message endpoints. It eliminates boilerplate code for endpoint configuration, transport setup, persistence, and lifecycle management.

## Key Features

- **NServiceBus Integration**: Pre-configured NServiceBus endpoint hosting with sensible defaults
- **SQL Persistence Support**: Built-in support for SQL Server saga persistence
- **Transport Abstraction**: Easy-to-override transport configuration for different messaging systems
- **Recoverability**: Configurable retry policies for immediate and delayed retries
- **Outbox Pattern**: Optional exactly-once message processing with outbox support
- **Lifecycle Management**: Proper endpoint startup and graceful shutdown handling
- **Inherits BaseHostedService**: Full access to Serilog logging, DI, and lifecycle hooks

## Quick Start

```csharp
public class OrderProcessingEndpoint : BaseEndpointService
{
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "OrderProcessing",
        EnableInstallers = true,
        MaxConcurrency = 4
    };

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString("host=localhost");
        
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(ProcessPayment), "PaymentService");
    }
}
```

## Architecture

QuickHostedEndpoint follows the same **Onion Architecture** principles as QuickHostedService:

```
QuickHostedEndpoint/
??? Core/                  # Domain exceptions
?   ??? Exceptions/        # EndpointConfigurationException
??? Application/           # BaseEndpointService implementation
??? Infrastructure/        # Configuration options
    ??? Configuration/     # EndpointOptions, SqlPersistenceOptions
```

## Configuration Options

### EndpointOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EndpointName` | `string?` | Class name | The NServiceBus endpoint name |
| `EnableInstallers` | `bool` | `true` | Auto-create queues and tables |
| `PurgeOnStartup` | `bool` | `false` | Clear queue on startup (dev only) |
| `ErrorQueue` | `string` | `"error"` | Failed message destination |
| `AuditQueue` | `string?` | `null` | Audit queue (null = disabled) |
| `MaxConcurrency` | `int` | `1` | Concurrent message processing |
| `ImmediateRetryCount` | `int` | `3` | Immediate retry attempts |
| `DelayedRetryCount` | `int` | `3` | Delayed retry attempts |
| `DelayedRetryTimeIncrease` | `TimeSpan` | `10s` | Delay increase between retries |
| `EnableOutbox` | `bool` | `false` | Enable outbox pattern |

### SqlPersistenceOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | `string?` | Required | SQL Server connection string |
| `Schema` | `string` | `"dbo"` | Database schema for tables |
| `TablePrefix` | `string?` | Endpoint name | Prefix for persistence tables |
| `EnableSagaPersistence` | `bool` | `true` | Enable saga state storage |
| `EnableSubscriptionStorage` | `bool` | `false` | Enable subscription storage |
| `SubscriptionCachePeriod` | `TimeSpan` | `5s` | Subscription cache duration |

## Documentation

- [Architecture Overview](ARCHITECTURE.md)
- [Usage Guide](USAGE.md)
- [API Reference](API.md)
- [Examples](../../src/examples/NServiceBusEndpoint/README.md)

## Requirements

- .NET 10.0 or later
- QuickHostedService
- NServiceBus 9.x
- NServiceBus.SqlPersistence (for saga support)
- Microsoft.Data.SqlClient (for SQL Server)
