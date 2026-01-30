# Usage Guide

This guide demonstrates how to use ClearHostedService in various scenarios.

## Table of Contents

1. [Basic Usage](#basic-usage)
2. [Dependency Injection](#dependency-injection)
3. [Logging Configuration](#logging-configuration)
4. [Lifecycle Hooks](#lifecycle-hooks)
5. [Error Handling](#error-handling)
6. [Advanced Scenarios](#advanced-scenarios)

## Basic Usage

### Simple Background Service

The most basic implementation requires only implementing the `ExecuteAsync` method:

```csharp
using ClearMeasure.HostedService;

public class SimpleWorker : ClearHostedService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Logger.Information("Worker running at: {Time}", DateTimeOffset.Now);
            await Task.Delay(10000, stoppingToken);
        }
    }
}
```

### Registering with Generic Host

```csharp
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<SimpleWorker>();
    })
    .Build();

await host.RunAsync();
```

## Dependency Injection

### Registering Dependencies

Override `RegisterDependencyInjection` to register your services:

```csharp
public class DataProcessorService : ClearHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        // Register your services
        services.AddScoped<IDataRepository, SqlDataRepository>();
        services.AddScoped<IDataProcessor, DataProcessor>();
        services.AddSingleton<IMessageQueue, RabbitMQQueue>();
        
        // Register HTTP clients
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // Register configuration
        services.Configure<DataProcessorOptions>(options =>
        {
            options.BatchSize = 100;
            options.MaxRetries = 3;
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Resolve services from the isolated ServiceProvider
        var dataProcessor = ServiceProvider.GetRequiredService<IDataProcessor>();
        var messageQueue = ServiceProvider.GetRequiredService<IMessageQueue>();
        
        await dataProcessor.ProcessDataAsync(stoppingToken);
    }
}
```

### Using Scoped Services

```csharp
public class ScopedServiceWorker : ClearHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IDatabaseContext, DatabaseContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create a new scope for each iteration
            using (var scope = ServiceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                await unitOfWork.ProcessBatchAsync();
                await unitOfWork.SaveChangesAsync();
            }
            
            await Task.Delay(5000, stoppingToken);
        }
    }
}
```

## Logging Configuration

### Customizing Serilog

Override `ConfigureLogging` to customize log configuration:

```csharp
public class CustomLoggingService : ClearHostedService
{
    protected override void ConfigureLogging(ILoggingBuilder builder)
    {
        // Call base to get default configuration
        base.ConfigureLogging(builder);
        
        // Add custom configuration
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "CustomService")
            .Enrich.WithProperty("Version", "1.0.0")
            .WriteTo.Console()
            .WriteTo.File(
                "logs/service-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.ApplicationInsights(
                new TelemetryConfiguration { InstrumentationKey = "your-key" },
                TelemetryConverter.Traces)
            .CreateLogger();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.Debug("This is a debug message");
        Logger.Information("Processing started");
        Logger.Warning("This is a warning");
        
        try
        {
            await DoWorkAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred during processing");
            throw;
        }
    }
}
```

### Structured Logging

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var orderId = "ORD-12345";
    var customerId = "CUST-789";
    var amount = 99.99m;
    
    // Use structured logging with property placeholders
    Logger.Information(
        "Processing order {OrderId} for customer {CustomerId} with amount {Amount:C}",
        orderId,
        customerId,
        amount);
    
    // This creates searchable, structured log entries in ApplicationInsights
}
```

## Lifecycle Hooks

### Startup Hook

Use `OnStartingAsync` to perform initialization before the main execution starts:

```csharp
public class InitializationService : ClearHostedService
{
    protected override async Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Performing startup initialization...");
        
        // Validate configuration
        var config = ServiceProvider.GetRequiredService<IConfiguration>();
        ValidateConfiguration(config);
        
        // Warm up caches
        var cache = ServiceProvider.GetRequiredService<ICache>();
        await cache.WarmUpAsync(cancellationToken);
        
        // Perform health checks
        var healthCheck = ServiceProvider.GetRequiredService<IHealthCheck>();
        var result = await healthCheck.CheckHealthAsync(cancellationToken);
        
        if (result.Status != HealthStatus.Healthy)
        {
            throw new Exception("Health check failed on startup");
        }
        
        Logger.Information("Startup initialization completed successfully");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Main execution logic
    }
}
```

### Shutdown Hook

Use `OnStoppingAsync` to perform cleanup before the service stops:

```csharp
public class CleanupService : ClearHostedService
{
    protected override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Performing shutdown cleanup...");
        
        // Flush pending messages
        var messageQueue = ServiceProvider.GetRequiredService<IMessageQueue>();
        await messageQueue.FlushAsync(cancellationToken);
        
        // Close connections gracefully
        var connectionManager = ServiceProvider.GetRequiredService<IConnectionManager>();
        await connectionManager.CloseAllAsync(cancellationToken);
        
        // Save state
        var stateManager = ServiceProvider.GetRequiredService<IStateManager>();
        await stateManager.SaveStateAsync(cancellationToken);
        
        Logger.Information("Shutdown cleanup completed");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Main execution logic
    }
}
```

## Error Handling

### Retry Logic

```csharp
public class ResilientService : ClearHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IDataService, DataService>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
                
                await ProcessWithRetryAsync(dataService, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Fatal error in processing loop");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ProcessWithRetryAsync(IDataService dataService, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                await dataService.ProcessAsync(cancellationToken);
                return; // Success
            }
            catch (TransientException ex)
            {
                retryCount++;
                Logger.Warning(ex, "Transient error occurred. Retry {RetryCount}/{MaxRetries}", retryCount, maxRetries);
                
                if (retryCount >= maxRetries)
                {
                    throw;
                }
                
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
```

### Circuit Breaker Pattern

```csharp
public class CircuitBreakerService : ClearHostedService
{
    private int consecutiveFailures = 0;
    private DateTime? circuitOpenedAt = null;
    private const int FailureThreshold = 5;
    private static readonly TimeSpan CircuitResetTime = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsCircuitOpen())
            {
                Logger.Warning("Circuit is open. Waiting before retry...");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }
            
            try
            {
                await ProcessDataAsync(stoppingToken);
                OnSuccess();
            }
            catch (Exception ex)
            {
                OnFailure(ex);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private bool IsCircuitOpen()
    {
        if (circuitOpenedAt.HasValue)
        {
            if (DateTime.UtcNow - circuitOpenedAt.Value > CircuitResetTime)
            {
                Logger.Information("Circuit reset time elapsed. Attempting to close circuit.");
                circuitOpenedAt = null;
                consecutiveFailures = 0;
                return false;
            }
            return true;
        }
        return false;
    }

    private void OnSuccess()
    {
        if (consecutiveFailures > 0)
        {
            Logger.Information("Operation succeeded. Resetting failure count.");
        }
        consecutiveFailures = 0;
        circuitOpenedAt = null;
    }

    private void OnFailure(Exception ex)
    {
        consecutiveFailures++;
        Logger.Error(ex, "Operation failed. Consecutive failures: {FailureCount}", consecutiveFailures);
        
        if (consecutiveFailures >= FailureThreshold)
        {
            circuitOpenedAt = DateTime.UtcNow;
            Logger.Error("Failure threshold reached. Opening circuit.");
        }
    }
}
```

## Advanced Scenarios

### Message Queue Consumer

```csharp
public class MessageQueueConsumer : ClearHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddSingleton<IMessageQueue, RabbitMQQueue>();
        services.AddScoped<IMessageHandler, MessageHandler>();
        services.Configure<QueueOptions>(options =>
        {
            options.QueueName = "my-queue";
            options.PrefetchCount = 10;
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = ServiceProvider.GetRequiredService<IMessageQueue>();
        await queue.ConnectAsync(stoppingToken);
        
        await foreach (var message in queue.ConsumeAsync(stoppingToken))
        {
            using var scope = ServiceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
            
            try
            {
                await handler.HandleAsync(message, stoppingToken);
                await queue.AckAsync(message);
                Logger.Information("Processed message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to process message {MessageId}", message.Id);
                await queue.NackAsync(message, requeue: true);
            }
        }
    }

    protected override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        var queue = ServiceProvider.GetRequiredService<IMessageQueue>();
        await queue.DisconnectAsync(cancellationToken);
    }
}
```

### Scheduled Task Service

```csharp
public class ScheduledTaskService : ClearHostedService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Schedule daily task at 2 AM
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var scheduledTime = DateTime.Today.AddHours(2);
            
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }
            
            var delay = scheduledTime - now;
            Logger.Information("Next execution scheduled for {ScheduledTime}", scheduledTime);
            
            await Task.Delay(delay, stoppingToken);
            
            if (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteScheduledTaskAsync(stoppingToken);
            }
        }
    }

    private async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Executing scheduled task at {Time}", DateTime.Now);
        
        using var scope = ServiceProvider.CreateScope();
        var taskExecutor = scope.ServiceProvider.GetRequiredService<ITaskExecutor>();
        
        await taskExecutor.ExecuteAsync(cancellationToken);
        
        Logger.Information("Scheduled task completed at {Time}", DateTime.Now);
    }
}
```

### Multi-Tenant Service

```csharp
public class MultiTenantService : ClearHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ITenantDataService, TenantDataService>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tenantResolver = ServiceProvider.GetRequiredService<ITenantResolver>();
        var tenants = await tenantResolver.GetAllTenantsAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var tasks = tenants.Select(tenant => ProcessTenantAsync(tenant, stoppingToken));
            await Task.WhenAll(tasks);
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        using var scope = ServiceProvider.CreateScope();
        var dataService = scope.ServiceProvider.GetRequiredService<ITenantDataService>();
        
        Logger.Information("Processing data for tenant {TenantId}", tenant.Id);
        
        try
        {
            await dataService.ProcessForTenantAsync(tenant.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error processing tenant {TenantId}", tenant.Id);
        }
    }
}
```
