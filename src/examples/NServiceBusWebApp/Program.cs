using NServiceBus;
using NServiceBusEndpoint.Messages;

var builder = WebApplication.CreateBuilder(args);

// Configure NServiceBus to send messages using Learning Transport
builder.Host.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration("NServiceBusWebApp");

    // Use Learning Transport - same as the OrderProcessingEndpoint
    var learningTransportDirectory = Path.Combine(
        Path.GetTempPath(),
        "NServiceBusEndpointExample",
        ".learningtransport");

    var transport = endpointConfiguration.UseTransport<LearningTransport>();
    transport.StorageDirectory(learningTransportDirectory);

    // Configure serialization to match the endpoint
    endpointConfiguration.UseSerialization<SystemJsonSerializer>();

    // This is a send-only endpoint (doesn't process messages)
    endpointConfiguration.SendOnly();

    // Route PlaceOrder commands to the OrderProcessing endpoint
    var routing = transport.Routing();
    routing.RouteToEndpoint(typeof(PlaceOrder), "OrderProcessing");

    return endpointConfiguration;
});

// Add services
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Minimal API endpoints
app.MapGet("/", () => Results.Content(GetIndexHtml(), "text/html"));

app.MapPost("/orders", async (IMessageSession messageSession, HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    
    var command = new PlaceOrder
    {
        OrderId = Guid.NewGuid(),
        CustomerId = form["customerId"].ToString(),
        Product = form["product"].ToString(),
        Quantity = int.Parse(form["quantity"].ToString()),
        TotalAmount = decimal.Parse(form["totalAmount"].ToString())
    };

    await messageSession.Send(command);

    return Results.Content(GetSuccessHtml(command), "text/html");
});

app.MapGet("/api/orders", async (IMessageSession messageSession) =>
{
    var command = new PlaceOrder
    {
        OrderId = Guid.NewGuid(),
        CustomerId = "API-" + Guid.NewGuid().ToString("N")[..8],
        Product = "API Widget",
        Quantity = 1,
        TotalAmount = 19.99m
    };

    await messageSession.Send(command);

    return Results.Ok(new
    {
        message = "Order placed successfully",
        orderId = command.OrderId,
        customerId = command.CustomerId,
        product = command.Product,
        quantity = command.Quantity,
        totalAmount = command.TotalAmount
    });
});

app.Run();

static string GetIndexHtml() => """
    <!DOCTYPE html>
    <html>
    <head>
        <title>NServiceBus Order Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 600px; margin: 50px auto; padding: 20px; }
            h1 { color: #333; }
            form { background: #f5f5f5; padding: 20px; border-radius: 8px; }
            label { display: block; margin-top: 10px; font-weight: bold; }
            input { width: 100%; padding: 8px; margin-top: 5px; border: 1px solid #ccc; border-radius: 4px; box-sizing: border-box; }
            button { margin-top: 20px; padding: 12px 24px; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer; }
            button:hover { background: #0056b3; }
            .info { background: #e7f3ff; padding: 10px; border-radius: 4px; margin-bottom: 20px; }
        </style>
    </head>
    <body>
        <h1>?? NServiceBus Order Demo</h1>
        <div class="info">
            <strong>Note:</strong> Make sure the NServiceBusEndpoint worker is running to process orders.
        </div>
        <form action="/orders" method="post">
            <label for="customerId">Customer ID</label>
            <input type="text" id="customerId" name="customerId" required value="CUST-001">
            
            <label for="product">Product</label>
            <input type="text" id="product" name="product" required value="Widget">
            
            <label for="quantity">Quantity</label>
            <input type="number" id="quantity" name="quantity" required value="5" min="1">
            
            <label for="totalAmount">Total Amount</label>
            <input type="number" id="totalAmount" name="totalAmount" required value="99.95" step="0.01" min="0">
            
            <button type="submit">Place Order</button>
        </form>
    </body>
    </html>
    """;

static string GetSuccessHtml(PlaceOrder command) => 
"""
<!DOCTYPE html>
<html>
<head>
    <title>Order Placed</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 50px auto; padding: 20px; }
        .success { background: #d4edda; padding: 20px; border-radius: 8px; border: 1px solid #c3e6cb; }
        h1 { color: #155724; }
        a { color: #007bff; }
        .details { margin-top: 15px; }
        .details dt { font-weight: bold; margin-top: 10px; }
    </style>
</head>
<body>
    <div class="success">
        <h1>? Order Placed Successfully!</h1>
        <p>Your order has been sent to the OrderProcessing endpoint.</p>
        <dl class="details">
            <dt>Order ID</dt>
            <dd>
""" + command.OrderId + """
</dd>
            <dt>Customer</dt>
            <dd>
""" + command.CustomerId + """
</dd>
            <dt>Product</dt>
            <dd>
""" + command.Product + """
</dd>
            <dt>Quantity</dt>
            <dd>
""" + command.Quantity + """
</dd>
            <dt>Total</dt>
            <dd>$
""" + command.TotalAmount.ToString("F2") + """
</dd>
        </dl>
    </div>
    <p style="margin-top: 20px;"><a href="/">? Place another order</a></p>
</body>
</html>
""";
