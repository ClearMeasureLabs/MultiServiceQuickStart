# Simple Worker Example

This example demonstrates the most basic usage of ClearHostedService.

## What It Does

The `SimpleWorkerService` is a background worker that:
- Runs continuously in a loop
- Logs a message every 5 seconds
- Uses lifecycle hooks for startup and shutdown
- Handles cancellation gracefully

## Key Concepts Demonstrated

1. **Basic Implementation**: Minimal code required to create a working hosted service
2. **Logging**: Using the built-in Serilog logger
3. **Cancellation Handling**: Respecting the cancellation token for graceful shutdown
4. **Lifecycle Hooks**: Using `OnStartingAsync` and `OnStoppingAsync`

## Running the Example

```bash
cd examples/SimpleWorker
dotnet run
```

Press `Ctrl+C` to stop the service gracefully.

## Expected Output

```
Starting SimpleWorker example...
Press Ctrl+C to stop.

[INF] Starting hosted service: SimpleWorkerService
[INF] Performing startup initialization...
[INF] Hosted service started successfully: SimpleWorkerService
[INF] SimpleWorkerService is starting...
[INF] Worker running - Iteration 1 at 01/15/2025 10:30:00 AM +00:00
[INF] Worker running - Iteration 2 at 01/15/2025 10:30:05 AM +00:00
[INF] Worker running - Iteration 3 at 01/15/2025 10:30:10 AM +00:00
^C
[INF] Stopping hosted service: SimpleWorkerService
[INF] SimpleWorkerService is stopping gracefully
[INF] Performing shutdown cleanup...
[INF] Hosted service stopped: SimpleWorkerService
```

## Code Walkthrough

### The Service Class

```csharp
public class SimpleWorkerService : ClearHostedService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Main execution loop
        while (!stoppingToken.IsCancellationRequested)
        {
            Logger.Information("Worker running...");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

### Registration

```csharp
services.AddHostedService<SimpleWorkerService>();
```

That's it! No additional configuration needed for basic scenarios.

## Next Steps

See other examples for:
- [DataProcessorService](../DataProcessorService) - Using dependency injection
- [MessageQueueConsumer](../MessageQueueConsumer) - Consuming messages
- [ScheduledTaskService](../ScheduledTaskService) - Time-based execution
