using NServiceBus;
using NServiceBusEndpoint.Messages;
using Serilog;

namespace NServiceBusEndpoint.Handlers;

/// <summary>
/// Handles the OrderPlaced event for logging/notification purposes.
/// </summary>
public class OrderPlacedHandler : IHandleMessages<OrderPlaced>
{
    private static readonly ILogger Logger = Log.ForContext<OrderPlacedHandler>();

    /// <summary>
    /// Handles the OrderPlaced event.
    /// </summary>
    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        Logger.Information(
            "Order notification: Order {OrderId} for customer {CustomerId} was placed at {PlacedAt}",
            message.OrderId,
            message.CustomerId,
            message.PlacedAt);

        // In a real application, you might:
        // - Send email notifications
        // - Update dashboards
        // - Trigger downstream processes

        return Task.CompletedTask;
    }
}
