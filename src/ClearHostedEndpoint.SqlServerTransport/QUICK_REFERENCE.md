# ClearHostedEndpoint.SqlServerTransport

## Summary

`ClearHostedEndpoint.SqlServerTransport` provides a streamlined, best-practice implementation of SQL Server transport for NServiceBus endpoints. It dramatically simplifies configuration while ensuring proper transaction coordination and preventing distributed transaction escalation.

## Quick Reference

### Installation

```bash
dotnet add package ClearMeasure.HostedEndpoint.SqlServerTransport
```

### Basic Usage

```csharp
using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.SqlServerTransport;

public class MyEndpoint : ClearHostedEndpoint
{
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // One line configuration!
        endpointConfiguration.UseSqlServerTransport(
            "Server=localhost;Database=MyApp;Integrated Security=true;");
    }
}
```

### Using StorageContext in Handlers

```csharp
public class OrderHandler : IHandleMessages<ProcessOrder>
{
    private readonly StorageContext _storage;

    public OrderHandler(StorageContext storage)
    {
        _storage = storage;
    }

    public async Task Handle(ProcessOrder message, IMessageHandlerContext context)
    {
        // All database operations use the same transaction as message processing
        await _storage.ExecuteAsync(
            "INSERT INTO Orders (OrderId, Total) VALUES (@OrderId, @Total)",
            new { message.OrderId, message.Total });

        var order = await _storage.QuerySingleAsync<Order>(
            "SELECT * FROM Orders WHERE OrderId = @OrderId",
            new { message.OrderId });
    }
}
```

## What It Does

### Automatic Configuration
- ? Sets `TransportTransactionMode.TransactionScope`
- ? Configures Outbox for exactly-once processing
- ? Registers `StorageContext` as transient dependency
- ? Adds `StorageContextBehavior` to pipeline
- ? Prevents distributed transaction escalation

### StorageContext Methods

| Method | Description |
|--------|-------------|
| `QueryAsync<T>` | Execute a query and return multiple results |
| `QuerySingleAsync<T>` | Execute a query and return a single result (throws if none) |
| `QuerySingleOrDefaultAsync<T>` | Execute a query and return a single result or default |
| `QueryFirstAsync<T>` | Execute a query and return the first result (throws if none) |
| `QueryFirstOrDefaultAsync<T>` | Execute a query and return the first result or default |
| `ExecuteAsync` | Execute a command and return rows affected |
| `ExecuteScalarAsync<T>` | Execute a command and return a scalar value |
| `QueryMultipleAsync` | Execute a query with multiple result sets |

All methods automatically use the transaction from the NServiceBus pipeline.

## Configuration Options

```csharp
var options = new SqlServerTransportOptions
{
    IsolationLevel = IsolationLevel.ReadCommitted,
    TransactionTimeout = TimeSpan.FromMinutes(2),
    DefaultSchema = "messaging",
    EnableNativeDelayedDelivery = true,
    OutboxDeduplicationPeriod = TimeSpan.FromDays(14),
    SubscriptionSettings = new SubscriptionSettings
    {
        TableName = "MySubscriptions",
        Schema = "messaging",
        EnableCaching = true
    }
};

endpointConfiguration.UseSqlServerTransport(connectionString, options);
```

## Architecture

### Transaction Flow

1. **NServiceBus receives message** ? Creates `TransactionScope`
2. **SQL Persistence opens connection** ? Enlists in `TransactionScope`
3. **StorageContextBehavior executes** ? Attaches connection/transaction to `StorageContext`
4. **Handler executes** ? Uses `StorageContext` for database operations
5. **Handler completes** ? Transaction commits (or rolls back on exception)

### Components

```
???????????????????????????????????????????????????
? NServiceBus Message Pipeline                     ?
?                                                   ?
?  ?????????????????????????????????????????      ?
?  ? TransactionScope (ambient)             ?      ?
?  ?  ???????????????????????????????      ?      ?
?  ?  ? SQL Persistence Session      ?      ?      ?
?  ?  ? - IDbConnection             ?      ?      ?
?  ?  ? - IDbTransaction            ?      ?      ?
?  ?  ???????????????????????????????      ?      ?
?  ?                ?                       ?      ?
?  ?  ???????????????????????????????      ?      ?
?  ?  ? StorageContextBehavior       ?      ?      ?
?  ?  ? - Resolves StorageContext    ?      ?      ?
?  ?  ? - Attaches connection/tx     ?      ?      ?
?  ?  ???????????????????????????????      ?      ?
?  ?                ?                       ?      ?
?  ?  ???????????????????????????????      ?      ?
?  ?  ? Message Handler              ?      ?      ?
?  ?  ? - Uses StorageContext        ?      ?      ?
?  ?  ? - Executes business logic    ?      ?      ?
?  ?  ????????????????????????????????      ?      ?
?  ??????????????????????????????????????????      ?
????????????????????????????????????????????????????
```

## Best Practices

### ? Do

- Inject `StorageContext` in message handler constructors
- Use `StorageContext` for all database operations in handlers
- Keep transactions short (< 1 minute)
- Use parameterized queries to prevent SQL injection

### ? Don't

- Don't create your own `SqlConnection` in handlers
- Don't create your own `TransactionScope` in handlers
- Don't dispose `StorageContext.Connection` or `StorageContext.Transaction`
- Don't access `StorageContext` outside of message handlers

## Common Scenarios

### Single Database Operation

```csharp
public async Task Handle(CreateOrder message, IMessageHandlerContext context)
{
    await _storage.ExecuteAsync(
        "INSERT INTO Orders (OrderId, Total) VALUES (@OrderId, @Total)",
        message);
}
```

### Multiple Operations

```csharp
public async Task Handle(ProcessOrder message, IMessageHandlerContext context)
{
    // Insert order
    await _storage.ExecuteAsync(
        "INSERT INTO Orders (OrderId, CustomerId, Total) VALUES (@OrderId, @CustomerId, @Total)",
        message);

    // Update inventory
    await _storage.ExecuteAsync(
        "UPDATE Inventory SET Quantity = Quantity - @Qty WHERE ProductId = @ProductId",
        new { message.ProductId, Qty = message.Quantity });

    // Send follow-up message (will be sent when transaction commits)
    await context.SendLocal(new OrderConfirmation { OrderId = message.OrderId });
}
```

### Complex Queries

```csharp
public async Task Handle(GetOrderDetails message, IMessageHandlerContext context)
{
    using var multi = await _storage.QueryMultipleAsync(@"
        SELECT * FROM Orders WHERE OrderId = @OrderId;
        SELECT * FROM OrderItems WHERE OrderId = @OrderId;
        SELECT * FROM Customers c 
        INNER JOIN Orders o ON c.CustomerId = o.CustomerId 
        WHERE o.OrderId = @OrderId",
        new { message.OrderId });

    var order = await multi.ReadSingleAsync<Order>();
    var items = await multi.ReadAsync<OrderItem>();
    var customer = await multi.ReadSingleAsync<Customer>();

    await context.Reply(new OrderDetailsResponse
    {
        Order = order,
        Items = items.ToList(),
        Customer = customer
    });
}
```

## Troubleshooting

### "StorageContext.Connection is not available"

**Cause**: `StorageContext` accessed outside of a message handler or before pipeline executes.

**Solution**: Only use `StorageContext` within `IHandleMessages<T>.Handle` methods.

### Distributed Transaction Escalation

**Cause**: Creating a new `SqlConnection` or opening multiple connections.

**Solution**: Always use `StorageContext` for database operations - never create your own connections.

### Transaction Timeout

**Cause**: Handler takes too long to execute.

**Solutions**:
- Reduce work in handler
- Increase `TransactionTimeout` in options
- Split large operations across multiple messages

## Files Created

This library includes the following files:

1. **ClearMeasure.HostedEndpoint.SqlServerTransport.csproj** - Project file
2. **StorageContext.cs** - Database operations context
3. **StorageContextBehavior.cs** - NServiceBus pipeline behavior
4. **SqlServerTransportExtensions.cs** - Configuration extensions and options
5. **Examples.cs** - Usage examples
6. **README.md** - Main documentation
7. **QUICK_REFERENCE.md** - This file
8. **GlobalUsings.cs** - Global usings

## Related Documentation

- [Full README](README.md) - Comprehensive documentation
- [ClearHostedEndpoint Documentation](../ClearHostedEndpoint/README.md)
- [NServiceBus SQL Transport](https://docs.particular.net/transports/sql/)

## Support

For issues, questions, or contributions, please refer to the main project documentation.

---

**Version**: 1.0.0  
**Last Updated**: 2025-01-29
