using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.SqlServerTransport;
using Microsoft.Extensions.Configuration;
using NServiceBus;

namespace YourNamespace;

/// <summary>
/// Example of a simple NServiceBus endpoint using SQL Server transport.
/// </summary>
public class SimpleEndpoint : ClearHostedEndpoint
{
    public SimpleEndpoint(IConfiguration configuration) : base(configuration)
    {
    }
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // Single line configuration with SQL Server transport
        endpointConfiguration.UseSqlServerTransport("Server=localhost;Database=MyApp;Integrated Security=true;");
    }
}

/// <summary>
/// Example with custom transport options.
/// </summary>
public class CustomEndpoint : ClearHostedEndpoint
{
    public CustomEndpoint(IConfiguration configuration) : base(configuration)
    {
    }
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var options = new SqlServerTransportOptions
        {
            IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
            TransactionTimeout = TimeSpan.FromMinutes(2),
            DefaultSchema = "messaging",
            EnableNativeDelayedDelivery = true,
            OutboxDeduplicationPeriod = TimeSpan.FromDays(14)
        };

        endpointConfiguration.UseSqlServerTransport(
            "Server=localhost;Database=MyApp;Integrated Security=true;",
            options);
    }
}

/// <summary>
/// Example message handler using StorageContext for database operations.
/// </summary>
public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
{
    private readonly StorageContext _storageContext;

    public OrderPlacedHandler(StorageContext storageContext)
    {
        _storageContext = storageContext;
    }

    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        // All database operations use the same transaction as message processing
        // This prevents distributed transaction escalation
        
        // Insert order
        await _storageContext.ExecuteAsync(
            "INSERT INTO Orders (OrderId, CustomerId, Total) VALUES (@OrderId, @CustomerId, @Total)",
            new { message.OrderId, message.CustomerId, message.Total });

        // Update customer
        await _storageContext.ExecuteAsync(
            "UPDATE Customers SET LastOrderDate = @Date WHERE CustomerId = @CustomerId",
            new { Date = DateTime.UtcNow, message.CustomerId });

        // Query for verification
        var orderCount = await _storageContext.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId",
            new { message.CustomerId });

        // Send a follow-up message
        await context.SendLocal(new OrderConfirmation { OrderId = message.OrderId });
    }
}

public class OrderPlaced : IMessage
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class OrderConfirmation : IMessage
{
    public Guid OrderId { get; set; }
}
