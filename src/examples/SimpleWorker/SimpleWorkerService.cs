using ClearMeasure.HostedService;
using Microsoft.Extensions.Configuration;

namespace SimpleWorker;

/// <summary>
/// A simple background worker that demonstrates basic usage of ClearHostedService.
/// </summary>
public class SimpleWorkerService : ClearHostedService
{
    public SimpleWorkerService(IConfiguration configuration) : base(configuration)
    {
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.Information("SimpleWorkerService is starting...");

        try
        {
            var iteration = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                iteration++;
                Logger.Information("Worker running - Iteration {Iteration} at {Time}", 
                    iteration, 
                    DateTimeOffset.Now);

                // Simulate some work
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the service is stopping
            Logger.Information("SimpleWorkerService is stopping gracefully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred in SimpleWorkerService");
            throw;
        }
    }

    public override Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Performing startup initialization...");
        return Task.CompletedTask;
    }

    public override Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Performing shutdown cleanup...");
        return Task.CompletedTask;
    }
}
