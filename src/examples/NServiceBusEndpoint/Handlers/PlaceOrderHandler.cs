using NServiceBus;
using NServiceBusEndpoint.Messages;
using Serilog;

namespace NServiceBusEndpoint.Handlers;

/// <summary>
/// Handles the PlaceOrder command.
/// </summary>
public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
{
    private static readonly ILogger Logger = Log.ForContext<PlaceOrderHandler>();

    /// <summary>
    /// Handles the PlaceOrder command by processing the order and publishing an OrderPlaced event.
    /// </summary>
    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        Logger.Information(
            "Processing order {OrderId} for customer {CustomerId}: {Quantity}x {Product} = ${TotalAmount}",
            message.OrderId,
            message.CustomerId,
            message.Quantity,
            message.Product,
            message.TotalAmount);

        // Simulate order processing
        await Task.Delay(500, context.CancellationToken);

        // Publish the OrderPlaced event
        var orderPlacedEvent = new OrderPlaced
        {
            OrderId = message.OrderId,
            CustomerId = message.CustomerId,
            PlacedAt = DateTimeOffset.UtcNow
        };

        await context.Publish(orderPlacedEvent);

        Logger.Information("Order {OrderId} has been placed successfully.", message.OrderId);
    }
}
