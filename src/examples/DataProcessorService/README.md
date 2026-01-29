# Data Processor Service Example

This example demonstrates advanced usage of QuickHostedService with dependency injection, scoped services, and batch processing.

## What It Does

The `DataProcessorHostedService`:
- Processes data records in batches
- Uses dependency injection for repositories and processors
- Demonstrates scoped service lifetime management
- Includes configuration through options pattern
- Shows proper error handling and logging

## Key Concepts Demonstrated

1. **Dependency Injection**: Registering and resolving services
2. **Scoped Services**: Creating scopes for each batch operation
3. **Configuration**: Using the Options pattern for configuration
4. **Batch Processing**: Processing data in controlled batches
5. **Error Handling**: Graceful error handling with retries
6. **Structured Logging**: Rich contextual logging

## Architecture

```
DataProcessorHostedService
??? IDataRepository (Singleton)
?   ??? InMemoryDataRepository
??? IDataProcessor (Scoped)
?   ??? DataProcessor
??? DataProcessorOptions (Configuration)
```

## Running the Example

```bash
cd examples/DataProcessorService
dotnet run
```

Press `Ctrl+C` to stop gracefully.

## Expected Output

```
===========================================
Data Processor Service Example
===========================================
This example demonstrates:
- Dependency injection
- Scoped services
- Configuration options
- Batch processing

Press Ctrl+C to stop.
===========================================

[INF] Starting hosted service: DataProcessorHostedService
[INF] DataProcessorHostedService is initializing...
[INF] Configuration: BatchSize=5, MaxRetries=3, ProcessingDelay=00:00:10
[INF] Hosted service started successfully: DataProcessorHostedService
[INF] DataProcessorHostedService started. Beginning data processing loop...
[INF] Processing batch of 5 records
[INF] Processing record 1: Record_1
[INF] Completed processing record 1. Age: 0.00 minutes
[INF] Processing record 2: Record_2
[INF] Completed processing record 2. Age: 1.00 minutes
...
[INF] Completed batch processing. Processed 5 records
[INF] Waiting 00:00:10 before next batch...
```

## Code Highlights

### Dependency Registration

```csharp
protected override void RegisterDependencyInjection(IServiceCollection services)
{
    // Configuration
    services.Configure<DataProcessorOptions>(options =>
    {
        options.BatchSize = 5;
        options.ProcessingDelay = TimeSpan.FromSeconds(10);
    });

    // Singleton repository (shared state)
    services.AddSingleton<IDataRepository, InMemoryDataRepository>();
    
    // Scoped processor (per-batch instance)
    services.AddScoped<IDataProcessor, DataProcessor>();
}
```

### Using Scoped Services

```csharp
// Create a new scope for each batch
using var scope = ServiceProvider.CreateScope();
var repository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
var processor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
```

### Batch Processing Loop

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    await ProcessBatchAsync(options, stoppingToken);
    await Task.Delay(options.ProcessingDelay, stoppingToken);
}
```

## Customization

You can customize the behavior by modifying `DataProcessorOptions`:

```csharp
services.Configure<DataProcessorOptions>(options =>
{
    options.BatchSize = 20;           // Process 20 records per batch
    options.MaxRetries = 5;           // Retry up to 5 times
    options.ProcessingDelay = TimeSpan.FromMinutes(1);  // Wait 1 minute between batches
});
```

## Real-World Applications

This pattern is suitable for:
- ETL (Extract, Transform, Load) processes
- Data migration jobs
- Report generation
- Batch email sending
- Database maintenance tasks
- Log aggregation and processing

## Next Steps

See other examples for:
- [MessageQueueConsumer](../MessageQueueConsumer) - Event-driven processing
- [ScheduledTaskService](../ScheduledTaskService) - Time-based scheduling
- [ResilientService](../ResilientService) - Retry and circuit breaker patterns
