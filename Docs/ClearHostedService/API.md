# API Reference

## ClearHostedService

The core abstract class that provides the foundation for all hosted services.

### Class Declaration

```csharp
public abstract class ClearHostedService : IHostedService, IDisposable
```

### Properties

#### ServiceProvider

```csharp
protected IServiceProvider ServiceProvider { get; }
```

Gets the isolated service provider for this hosted service instance. Use this to resolve dependencies registered in `RegisterDependencyInjection`.

**Example:**
```csharp
var myService = ServiceProvider.GetRequiredService<IMyService>();
```

#### Logger

```csharp
protected ILogger Logger { get; }
```

Gets the Serilog logger instance configured for this hosted service. Use this for all logging operations.

**Example:**
```csharp
Logger.Information("Processing started");
Logger.Error(ex, "An error occurred");
```

### Methods

#### RegisterDependencyInjection (Virtual)

```csharp
protected virtual void RegisterDependencyInjection(IServiceCollection services)
```

Override this method to register application-specific dependencies into the service collection.

**Parameters:**
- `services`: The service collection to register dependencies into

**Example:**
```csharp
protected override void RegisterDependencyInjection(IServiceCollection services)
{
    services.AddScoped<IMyService, MyService>();
    services.AddSingleton<ICache, MemoryCache>();
}
```

**Notes:**
- Called once during service startup, before `ExecuteAsync`
- Services registered here are isolated to this hosted service instance
- Common services (logging, configuration) are already registered by the base class

---

#### ConfigureLogging (Virtual)

```csharp
protected virtual void ConfigureLogging(ILoggingBuilder builder)
```

Override this method to customize the logging configuration.

**Parameters:**
- `builder`: The logging builder to configure

**Example:**
```csharp
protected override void ConfigureLogging(ILoggingBuilder builder)
{
    base.ConfigureLogging(builder);
    
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/myservice-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}
```

**Notes:**
- Call `base.ConfigureLogging(builder)` to include default configuration
- Default configuration includes Console and File sinks
- ApplicationInsights is configured if instrumentation key is provided

---

#### OnStartingAsync (Virtual)

```csharp
protected virtual Task OnStartingAsync(CancellationToken cancellationToken)
```

Override this method to perform initialization logic before `ExecuteAsync` begins.

**Parameters:**
- `cancellationToken`: A cancellation token that can be used to cancel the startup operation

**Returns:**
- A task representing the asynchronous operation

**Example:**
```csharp
protected override async Task OnStartingAsync(CancellationToken cancellationToken)
{
    Logger.Information("Performing startup validation...");
    
    var config = ServiceProvider.GetRequiredService<IConfiguration>();
    ValidateConfiguration(config);
    
    await WarmUpCacheAsync(cancellationToken);
}
```

**Notes:**
- Called after dependency injection is configured
- Called before `ExecuteAsync` starts
- Exceptions thrown here will prevent the service from starting

---

#### OnStoppingAsync (Virtual)

```csharp
protected virtual Task OnStoppingAsync(CancellationToken cancellationToken)
```

Override this method to perform cleanup logic during service shutdown.

**Parameters:**
- `cancellationToken`: A cancellation token that can be used to cancel the shutdown operation

**Returns:**
- A task representing the asynchronous operation

**Example:**
```csharp
protected override async Task OnStoppingAsync(CancellationToken cancellationToken)
{
    Logger.Information("Flushing pending operations...");
    
    var messageQueue = ServiceProvider.GetRequiredService<IMessageQueue>();
    await messageQueue.FlushAsync(cancellationToken);
    
    Logger.Information("Cleanup completed");
}
```

**Notes:**
- Called after `ExecuteAsync` has stopped
- Called before resources are disposed
- Should complete quickly to avoid delaying shutdown

---

#### ExecuteAsync (Abstract)

```csharp
protected abstract Task ExecuteAsync(CancellationToken stoppingToken)
```

Implement this method to define the main execution logic for your hosted service.

**Parameters:**
- `stoppingToken`: A cancellation token that is triggered when the service should stop

**Returns:**
- A task representing the asynchronous operation

**Example:**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await DoWorkAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
    }
}
```

**Notes:**
- This method runs on a background thread
- Should respect the `stoppingToken` to enable graceful shutdown
- Exceptions thrown here will be logged but won't crash the application
- For continuous operation, use a while loop with cancellation token checks

---

#### StartAsync (IHostedService)

```csharp
public Task StartAsync(CancellationToken cancellationToken)
```

**?? Do not override this method.** It is implemented by the base class to orchestrate the startup sequence.

**Startup Sequence:**
1. Configure logging
2. Build service collection
3. Call `RegisterDependencyInjection`
4. Build service provider
5. Call `OnStartingAsync`
6. Start `ExecuteAsync` on background thread

---

#### StopAsync (IHostedService)

```csharp
public Task StopAsync(CancellationToken cancellationToken)
```

**?? Do not override this method.** It is implemented by the base class to orchestrate the shutdown sequence.

**Shutdown Sequence:**
1. Signal cancellation token
2. Wait for `ExecuteAsync` to complete (with timeout)
3. Call `OnStoppingAsync`
4. Dispose service provider
5. Log completion

---

#### Dispose

```csharp
public void Dispose()
```

Disposes of resources used by the hosted service. Called automatically by the .NET host.

**Notes:**
- Disposes the service provider
- Disposes any other managed resources
- Do not call this method directly

---

## Interfaces

### IHostedServiceLifecycle

Defines lifecycle hooks for hosted services.

```csharp
public interface IHostedServiceLifecycle
{
    Task OnStartingAsync(CancellationToken cancellationToken);
    Task OnStoppingAsync(CancellationToken cancellationToken);
}
```

---

### IServiceRegistration

Defines the contract for dependency registration.

```csharp
public interface IServiceRegistration
{
    void RegisterDependencies(IServiceCollection services);
}
```

---

## Configuration

### LoggingOptions

Configuration options for logging.

```csharp
public class LoggingOptions
{
    public string LogLevel { get; set; } = "Information";
    public string LogDirectory { get; set; } = "logs";
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public bool EnableApplicationInsights { get; set; } = false;
    public string? ApplicationInsightsInstrumentationKey { get; set; }
}
```

---

### HostedServiceOptions

Configuration options for hosted services.

```csharp
public class HostedServiceOptions
{
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableDetailedErrors { get; set; } = false;
}
```

---

## Exceptions

### HostedServiceException

Base exception for hosted service-related errors.

```csharp
public class HostedServiceException : Exception
{
    public HostedServiceException(string message) : base(message) { }
    public HostedServiceException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

---

### ServiceRegistrationException

Thrown when there's an error during service registration.

```csharp
public class ServiceRegistrationException : HostedServiceException
{
    public ServiceRegistrationException(string message) : base(message) { }
    public ServiceRegistrationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

---

## Best Practices

### 1. Always Check Cancellation Token

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // Your work here
        await Task.Delay(1000, stoppingToken);
    }
}
```

### 2. Use Scopes for Scoped Services

```csharp
using var scope = ServiceProvider.CreateScope();
var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
```

### 3. Log Structured Data

```csharp
Logger.Information("Processing {RecordCount} records for {CustomerId}", count, customerId);
```

### 4. Handle Exceptions Appropriately

```csharp
try
{
    await ProcessDataAsync(stoppingToken);
}
catch (TransientException ex)
{
    Logger.Warning(ex, "Transient error, will retry");
    // Retry logic
}
catch (Exception ex)
{
    Logger.Error(ex, "Fatal error in processing");
    throw;
}
```

### 5. Dispose Resources Properly

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using var httpClient = new HttpClient();
    // Use httpClient
}
```
