using NServiceBus;

namespace NServiceBusEndpoint.Messages;

/// <summary>
/// Event published when an order has been placed successfully.
/// </summary>
public class OrderPlaced : IEvent
{
    /// <summary>
    /// Gets or sets the unique order identifier.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the order was placed.
    /// </summary>
    public DateTimeOffset PlacedAt { get; set; }
}
