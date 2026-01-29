using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBusEndpoint;

// Create and run the host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register the NServiceBus endpoint as a hosted service
        services.AddHostedService<OrderProcessingEndpoint>();
    })
    .Build();

Console.WriteLine("Starting NServiceBus Endpoint example...");
Console.WriteLine("This endpoint uses Learning Transport (file-based, for development only).");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

await host.RunAsync();
