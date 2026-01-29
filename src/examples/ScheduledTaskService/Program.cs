using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScheduledTaskService;

Console.WriteLine("===========================================");
Console.WriteLine("Scheduled Task Service Example");
Console.WriteLine("===========================================");
Console.WriteLine("This example demonstrates:");
Console.WriteLine("- Time-based task scheduling");
Console.WriteLine("- Calculating next run time");
Console.WriteLine("- Daily/weekly scheduling");
Console.WriteLine("- Multiple task execution");
Console.WriteLine();
Console.WriteLine("Note: For demo purposes, the task runs");
Console.WriteLine("every minute. In production, you would");
Console.WriteLine("schedule it for specific times like 2 AM.");
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine("===========================================");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ScheduledTaskHostedService>();
    })
    .Build();

await host.RunAsync();
