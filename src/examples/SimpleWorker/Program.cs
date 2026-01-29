using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleWorker;

// Create and run the host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register the hosted service
        services.AddHostedService<SimpleWorkerService>();
    })
    .Build();

Console.WriteLine("Starting SimpleWorker example...");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

await host.RunAsync();
