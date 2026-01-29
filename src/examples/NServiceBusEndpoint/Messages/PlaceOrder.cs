using NServiceBus;

namespace NServiceBusEndpoint.Messages;

/// <summary>
/// Command to place a new order.
/// </summary>
public class PlaceOrder : ICommand
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
    /// Gets or sets the product being ordered.
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity to order.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the total amount for the order.
    /// </summary>
    public decimal TotalAmount { get; set; }
}
