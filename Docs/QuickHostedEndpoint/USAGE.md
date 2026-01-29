# QuickHostedEndpoint Usage Guide

This guide covers common usage patterns for the QuickHostedEndpoint library.

## Basic Endpoint

The simplest endpoint implementation requires only transport configuration:

```csharp
public class MyEndpoint : BaseEndpointService
{
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory(Path.Combine(Path.GetTempPath(), "myendpoint"));
    }
}

// Program.cs
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddHostedService<MyEndpoint>())
    .Build();

await host.RunAsync();
```

## Endpoint with Custom Options

Configure endpoint behavior through `EndpointOptions`:

```csharp
public class OrderProcessingEndpoint : BaseEndpointService
{
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "Sales.OrderProcessing",
        EnableInstallers = true,
        MaxConcurrency = 8,
        ImmediateRetryCount = 2,
        DelayedRetryCount = 5,
        DelayedRetryTimeIncrease = TimeSpan.FromSeconds(30),
        ErrorQueue = "sales-errors",
        AuditQueue = "sales-audit"
    };

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString("host=localhost");
    }
}
```

## Endpoint with SQL Persistence

Enable saga persistence for long-running workflows:

```csharp
public class SagaEndpoint : BaseEndpointService
{
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "OrderSaga",
        EnableOutbox = true  // Recommended with SQL persistence
    };

    protected override SqlPersistenceOptions? SqlPersistenceOptions { get; } = new()
    {
        ConnectionString = "Server=.;Database=NServiceBus;Integrated Security=true;TrustServerCertificate=true",
        Schema = "saga",
        TablePrefix = "Order",
        EnableSagaPersistence = true,
        EnableSubscriptionStorage = false
    };

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString("Server=.;Database=NServiceBus;Integrated Security=true");
    }
}
```

## Message Handlers

Create handlers for your messages in the same assembly:

```csharp
// Commands
public class PlaceOrder : ICommand
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

// Events
public class OrderPlaced : IEvent
{
    public Guid OrderId { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
}

// Handler
public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
{
    private readonly ILogger<PlaceOrderHandler> _logger;

    public PlaceOrderHandler(ILogger<PlaceOrderHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Processing order {OrderId}", message.OrderId);
        
        // Process the order...
        
        await context.Publish(new OrderPlaced
        {
            OrderId = message.OrderId,
            PlacedAt = DateTimeOffset.UtcNow
        });
    }
}
```

## Message Routing

Configure routing in `ConfigureTransport`:

```csharp
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
    transport.ConnectionString("host=localhost");
    
    var routing = transport.Routing();
    
    // Route commands to specific endpoints
    routing.RouteToEndpoint(typeof(ProcessPayment), "Payments");
    routing.RouteToEndpoint(typeof(ShipOrder), "Shipping");
    
    // Route all commands in an assembly
    routing.RouteToEndpoint(typeof(PlaceOrder).Assembly, "Sales");
}
```

## Dependency Injection

Register services that handlers can consume:

```csharp
public class OrderEndpoint : BaseEndpointService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, SqlOrderRepository>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<IEmailService, SmtpEmailService>();
    }

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // Transport configuration...
    }
}

// Handler with injected dependencies
public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
{
    private readonly IOrderRepository _repository;
    private readonly IPaymentGateway _payments;

    public PlaceOrderHandler(IOrderRepository repository, IPaymentGateway payments)
    {
        _repository = repository;
        _payments = payments;
    }

    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        var order = await _repository.GetAsync(message.OrderId);
        await _payments.ChargeAsync(order);
    }
}
```

## Lifecycle Hooks

Use lifecycle methods for initialization and cleanup:

```csharp
public class MonitoredEndpoint : BaseEndpointService
{
    public override async Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Endpoint starting - validating configuration");
        
        // Validate connections, warm up caches, etc.
        await ValidateDatabaseConnectionAsync(cancellationToken);
        await WarmUpCacheAsync(cancellationToken);
    }

    public override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Endpoint stopping - flushing pending work");
        
        // Flush queues, save state, etc.
        await FlushPendingWorkAsync(cancellationToken);
        
        await base.OnStoppingAsync(cancellationToken); // Important: calls endpoint.Stop()
    }

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // Transport configuration...
    }
}
```

## Custom Serialization

Override `ConfigureSerialization` to use a different serializer:

```csharp
protected override void ConfigureSerialization(EndpointConfiguration endpointConfiguration)
{
    // Use Newtonsoft.Json instead of System.Text.Json
    var serialization = endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
    serialization.Settings(new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    });
}
```

## Custom Recoverability

Override `ConfigureRecoverability` for custom retry behavior:

```csharp
protected override void ConfigureRecoverability(EndpointConfiguration endpointConfiguration)
{
    endpointConfiguration.Recoverability()
        .Immediate(immediate => immediate
            .NumberOfRetries(5))
        .Delayed(delayed => delayed
            .NumberOfRetries(3)
            .TimeIncrease(TimeSpan.FromMinutes(1)))
        .AddUnrecoverableException<ValidationException>()
        .CustomPolicy((config, context) =>
        {
            if (context.Exception is TransientException)
            {
                return RecoverabilityAction.ImmediateRetry();
            }
            return RecoverabilityAction.MoveToError("custom-errors");
        });
}
```

## Sending Messages from the Endpoint

Access `EndpointInstance` to send messages from within the endpoint:

```csharp
public class ScheduledOrderEndpoint : BaseEndpointService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Send a scheduled check every minute
            await EndpointInstance.Send(new CheckPendingOrders());
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // Transport configuration...
    }
}
```

## Integration with ASP.NET Core

For web applications that need to send messages, use `NServiceBus.Extensions.Hosting`:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration("WebApp");
    endpointConfiguration.UseTransport<LearningTransport>();
    endpointConfiguration.SendOnly(); // Web apps typically only send
    
    var routing = endpointConfiguration.UseTransport<LearningTransport>().Routing();
    routing.RouteToEndpoint(typeof(PlaceOrder), "OrderProcessing");
    
    return endpointConfiguration;
});

var app = builder.Build();

app.MapPost("/orders", async (PlaceOrder command, IMessageSession session) =>
{
    await session.Send(command);
    return Results.Accepted();
});

app.Run();
```

## Testing

Test your handlers in isolation:

```csharp
[Fact]
public async Task PlaceOrderHandler_PublishesOrderPlacedEvent()
{
    // Arrange
    var handler = new PlaceOrderHandler(
        Mock.Of<ILogger<PlaceOrderHandler>>());
    
    var context = new TestableMessageHandlerContext();
    var command = new PlaceOrder { OrderId = Guid.NewGuid() };
    
    // Act
    await handler.Handle(command, context);
    
    // Assert
    var published = context.PublishedMessages.Single();
    var orderPlaced = published.Message as OrderPlaced;
    orderPlaced.Should().NotBeNull();
    orderPlaced!.OrderId.Should().Be(command.OrderId);
}
```

## Common Patterns

### Fire and Forget
```csharp
await EndpointInstance.Send(new ProcessInBackground { Data = data });
```

### Request/Response
```csharp
var response = await EndpointInstance.Request<OrderStatus>(
    new GetOrderStatus { OrderId = orderId });
```

### Publish/Subscribe
```csharp
// Publisher
await context.Publish(new OrderShipped { OrderId = orderId });

// Subscriber (in another endpoint)
public class OrderShippedHandler : IHandleMessages<OrderShipped>
{
    public Task Handle(OrderShipped message, IMessageHandlerContext context)
    {
        // React to the event
    }
}
```
