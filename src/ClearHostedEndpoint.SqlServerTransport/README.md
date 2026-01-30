# ClearHostedEndpoint.SqlServerTransport

Standardized SQL Server Transport configuration for NServiceBus with transaction management and storage context.

## Overview

`ClearHostedEndpoint.SqlServerTransport` provides a streamlined, best-practice implementation of SQL Server transport for NServiceBus endpoints. It eliminates boilerplate code, ensures proper transaction management, and prevents distributed transaction escalation.

## Key Features

- **Single-line transport configuration** - Configure SQL Server transport with one method call
- **Automatic transaction coordination** - Uses `TransactionScope` to ensure transport and persistence share the same transaction
- **StorageContext integration** - Inject `StorageContext` in handlers for database operations within the message transaction
- **Prevents distributed transactions** - All operations participate in the same local SQL transaction
- **Automatic Outbox configuration** - Ensures exactly-once message processing
- **Dapper integration** - Provides all Dapper extension methods through `StorageContext`

## Installation

```bash
dotnet add package ClearMeasure.HostedEndpoint.SqlServerTransport
```

## Quick Start

### 1. Configure Transport

In your `ClearHostedEndpoint` implementation, configure the transport with a single line:

```csharp
using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.SqlServerTransport;

public class MyEndpoint : ClearHostedEndpoint
{
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseSqlServerTransport(
            "Server=localhost;Database=MyApp;Integrated Security=true;");
    }
}
```

### 2. Use StorageContext in Handlers

Inject `StorageContext` into your message handlers to execute database operations:

```csharp
public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
{
    private readonly StorageContext _storageContext;

    public OrderPlacedHandler(StorageContext storageContext)
    {
        _storageContext = storageContext;
    }

    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        // All operations use the same transaction as message processing
        await _storageContext.ExecuteAsync(
            "INSERT INTO Orders (OrderId, Total) VALUES (@OrderId, @Total)",
            new { message.OrderId, message.Total });

        // Query data
        var order = await _storageContext.QuerySingleAsync<Order>(
            "SELECT * FROM Orders WHERE OrderId = @OrderId",
            new { message.OrderId });
    }
}
```

## How It Works

### Transaction Management

The library ensures all operations participate in a single SQL transaction:

1. **TransactionScope Mode** - Transport uses `TransactionScope` transaction mode
2. **Synchronized Session** - NServiceBus SQL Persistence provides a synchronized storage session
3. **StorageContextBehavior** - A pipeline behavior attaches the session's `IDbConnection` and `IDbTransaction` to `StorageContext`
4. **Handler Operations** - All `StorageContext` operations use the attached transaction

This architecture prevents escalation to MSDTC distributed transactions.

### Components

#### StorageContext

Provides Dapper-based database operations within the message transaction:

```csharp
public class StorageContext
{
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, ...)
    public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, ...)
    public Task<T> QuerySingleAsync<T>(string sql, object? param = null, ...)
    public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, ...)
    public Task<T> QueryFirstAsync<T>(string sql, object? param = null, ...)
    public Task<int> ExecuteAsync(string sql, object? param = null, ...)
    public Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, ...)
    public Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, ...)
}
```

#### StorageContextBehavior

NServiceBus pipeline behavior that:
- Resolves `StorageContext` from the DI container (transient)
- Attaches the synchronized storage session's connection and transaction
- Ensures handlers can use `StorageContext` safely

#### SqlServerTransportExtensions

Extension methods for configuring the transport:

```csharp
public static TransportExtensions<SqlServerTransport> UseSqlServerTransport(
    this EndpointConfiguration endpointConfiguration,
    string connectionString,
    SqlServerTransportOptions? options = null)
```

## Advanced Configuration

### Custom Options

```csharp
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var options = new SqlServerTransportOptions
    {
        IsolationLevel = IsolationLevel.ReadCommitted,
        TransactionTimeout = TimeSpan.FromMinutes(2),
        DefaultSchema = "messaging",
        EnableNativeDelayedDelivery = true,
        OutboxDeduplicationPeriod = TimeSpan.FromDays(14)
    };

    endpointConfiguration.UseSqlServerTransport(
        "Server=localhost;Database=MyApp;Integrated Security=true;",
        options);
}
```

### Multi-Schema Support

```csharp
var options = new SqlServerTransportOptions
{
    DefaultSchema = "messaging",
    QueueSchemaSettings = new QueueSchemaSettings
    {
        QueueName = "MyEndpoint",
        Schema = "custom_schema"
    }
};
```

### Connection Factory

```csharp
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    endpointConfiguration.UseSqlServerTransport(
        () => new SqlConnection("Server=localhost;Database=MyApp;Integrated Security=true;"));
}
```

## Configuration Options

### SqlServerTransportOptions

| Property | Default | Description |
|----------|---------|-------------|
| `IsolationLevel` | `ReadCommitted` | Transaction isolation level |
| `TransactionTimeout` | 1 minute | Maximum transaction duration |
| `DefaultSchema` | `null` (uses `dbo`) | Default schema for queues |
| `QueueSchemaSettings` | `null` | Per-queue schema configuration |
| `EnableNativeDelayedDelivery` | `true` | Use SQL Server native delayed delivery |
| `OutboxDeduplicationPeriod` | 7 days | How long to keep outbox deduplication data |

## Best Practices

### ? Do

- Inject `StorageContext` as a constructor parameter in handlers
- Use `StorageContext` for all database operations in handlers
- Keep transactions short (< 1 minute)
- Use appropriate isolation levels for your use case

### ? Don't

- Don't create your own `SqlConnection` or `DbTransaction` in handlers
- Don't use `TransactionScope` directly in handlers (already managed)
- Don't access `StorageContext.Connection` or `StorageContext.Transaction` outside message handlers
- Don't dispose the connection or transaction (managed by NServiceBus)

## Complete Example

```csharp
using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.SqlServerTransport;
using NServiceBus;

// Endpoint configuration
public class OrderProcessingEndpoint : ClearHostedEndpoint
{
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseSqlServerTransport(
            "Server=localhost;Database=OrderProcessing;Integrated Security=true;");
    }

    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<INotificationService, NotificationService>();
    }
}

// Message handler with StorageContext
public class ProcessOrderHandler : IHandleMessages<ProcessOrder>
{
    private readonly StorageContext _storageContext;
    private readonly INotificationService _notificationService;

    public ProcessOrderHandler(
        StorageContext storageContext,
        INotificationService notificationService)
    {
        _storageContext = storageContext;
        _notificationService = notificationService;
    }

    public async Task Handle(ProcessOrder message, IMessageHandlerContext context)
    {
        // Insert order - participates in message transaction
        await _storageContext.ExecuteAsync(
            @"INSERT INTO Orders (OrderId, CustomerId, Total, Status, CreatedAt)
              VALUES (@OrderId, @CustomerId, @Total, @Status, @CreatedAt)",
            new
            {
                message.OrderId,
                message.CustomerId,
                message.Total,
                Status = "Processing",
                CreatedAt = DateTime.UtcNow
            });

        // Update inventory - same transaction
        await _storageContext.ExecuteAsync(
            @"UPDATE Inventory 
              SET Quantity = Quantity - @Quantity 
              WHERE ProductId = @ProductId",
            new { message.ProductId, message.Quantity });

        // Query customer info - same transaction
        var customer = await _storageContext.QuerySingleAsync<Customer>(
            "SELECT * FROM Customers WHERE CustomerId = @CustomerId",
            new { message.CustomerId });

        // Send notification
        await _notificationService.NotifyCustomerAsync(customer.Email, message.OrderId);

        // Publish event - will be sent when transaction commits
        await context.Publish(new OrderProcessed
        {
            OrderId = message.OrderId,
            ProcessedAt = DateTime.UtcNow
        });
    }
}

// Messages
public class ProcessOrder : ICommand
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class OrderProcessed : IEvent
{
    public Guid OrderId { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class Customer
{
    public string CustomerId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

## Troubleshooting

### "StorageContext.Connection is not available"

This error occurs when:
- `StorageContext` is accessed outside a message handler
- `StorageContextBehavior` is not registered (should be automatic)
- Message processing has not started

**Solution**: Only use `StorageContext` within message handler methods.

### Distributed Transaction Escalation

If you see MSDTC errors:
- Ensure you're using `StorageContext` for all database operations
- Don't create separate `SqlConnection` instances
- Verify `TransactionScope` mode is enabled (automatic with `UseSqlServerTransport`)

### Transaction Timeout

If transactions are timing out:
- Reduce the amount of work in handlers
- Increase `SqlServerTransportOptions.TransactionTimeout`
- Consider splitting large operations across multiple messages

## Performance Considerations

- `StorageContext` is transient - a new instance is created per message
- Connection and transaction are reused from the NServiceBus session
- Dapper provides high-performance parameter mapping and object materialization
- All operations are async for optimal resource utilization

## Related Documentation

- [ClearHostedEndpoint Documentation](../ClearHostedEndpoint/README.md)
- [NServiceBus SQL Transport](https://docs.particular.net/transports/sql/)
- [NServiceBus SQL Persistence](https://docs.particular.net/persistence/sql/)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)

## License

[Your License Here]

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.
