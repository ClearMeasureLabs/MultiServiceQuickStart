using DataProcessorService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("===========================================");
Console.WriteLine("Data Processor Service Example");
Console.WriteLine("===========================================");
Console.WriteLine("This example demonstrates:");
Console.WriteLine("- Dependency injection");
Console.WriteLine("- Scoped services");
Console.WriteLine("- Configuration options");
Console.WriteLine("- Batch processing");
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine("===========================================");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<DataProcessorHostedService>();
    })
    .Build();

await host.RunAsync();
