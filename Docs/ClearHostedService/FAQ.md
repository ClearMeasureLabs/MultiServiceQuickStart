# Frequently Asked Questions (FAQ)

## General Questions

### What is ClearHostedService?

ClearHostedService is an opinionated base implementation of `IHostedService` that provides a foundation for building production-ready background services in .NET. It eliminates boilerplate code by providing pre-configured logging, dependency injection, and lifecycle management.

### When should I use ClearHostedService?

Use ClearHostedService when you need to:
- Build background workers or daemon processes
- Create scheduled tasks
- Consume message queues
- Process data in batches
- Run microservices or standalone applications

### When should I NOT use ClearHostedService?

Don't use it for:
- Web APIs or websites (use ASP.NET Core instead)
- Simple scripts that don't need logging or DI
- Very performance-critical scenarios where overhead matters
- Projects that can't use .NET 10.0

---

## Architecture Questions

### Why does each service have its own ServiceProvider?

Isolated service collections provide:
- **Independence**: Services don't affect each other
- **Clarity**: Dependencies are explicit per service
- **Testing**: Easier to mock and test
- **Safety**: No accidental cross-service dependencies

See [DESIGN_DECISIONS.md](DESIGN_DECISIONS.md#1-isolated-service-collections) for details.

### Why Onion Architecture?

Onion Architecture provides:
- Clear separation of concerns
- Testable code
- Maintainable codebase
- Flexible infrastructure

See [ARCHITECTURE.md](ARCHITECTURE.md#onion-architecture) for more information.

### Can I use a shared ServiceProvider?

No, by design each service is isolated. If you need shared state, consider:
- Using a shared database or cache
- Publishing events via a message bus
- Using shared files or distributed locks

---

## Usage Questions

### How do I register dependencies?

Override the `RegisterDependencyInjection` method:

```csharp
protected override void RegisterDependencyInjection(IServiceCollection services)
{
    services.AddScoped<IMyService, MyService>();
    services.AddSingleton<ICache, MemoryCache>();
}
```

See [USAGE.md#dependency-injection](USAGE.md#dependency-injection) for examples.

### How do I use scoped services?

Create a scope within `ExecuteAsync`:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using var scope = ServiceProvider.CreateScope();
    var scopedService = scope.ServiceProvider.GetRequiredService<IMyScopedService>();
    await scopedService.DoWorkAsync(stoppingToken);
}
```

### How do I configure logging?

Override the `ConfigureLogging` method:

```csharp
protected override void ConfigureLogging(ILoggingBuilder builder)
{
    base.ConfigureLogging(builder);
    
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/myapp-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}
```

See [USAGE.md#logging-configuration](USAGE.md#logging-configuration) for more.

### How do I handle application shutdown?

Check the `CancellationToken` in your loop:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await DoWorkAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

For cleanup, use `OnStoppingAsync`:

```csharp
protected override async Task OnStoppingAsync(CancellationToken cancellationToken)
{
    // Cleanup code here
}
```

---

## Configuration Questions

### How do I use appsettings.json?

Inject `IConfiguration` in your services:

```csharp
protected override void RegisterDependencyInjection(IServiceCollection services)
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();
    
    services.AddSingleton<IConfiguration>(configuration);
    services.Configure<MyOptions>(configuration.GetSection("MyOptions"));
}
```

### How do I use environment-specific settings?

```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
```

### How do I use secrets in production?

Options:
1. **Environment Variables**: `Environment.GetEnvironmentVariable("SECRET")`
2. **Azure Key Vault**: Use `AddAzureKeyVault()` configuration provider
3. **User Secrets**: For development, use `dotnet user-secrets`
4. **Configuration Providers**: Any custom provider

Never hardcode secrets!

---

## Testing Questions

### How do I unit test my service?

Mock dependencies and test business logic:

```csharp
[Fact]
public async Task MyService_ProcessesData_Successfully()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    mockService.Setup(x => x.GetDataAsync()).ReturnsAsync(testData);
    
    var service = new MyHostedService();
    // Inject mock through DI or constructor
    
    // Act
    await service.StartAsync(CancellationToken.None);
    
    // Assert
    mockService.Verify(x => x.GetDataAsync(), Times.Once);
}
```

### How do I integration test my service?

Use real dependencies with test containers:

```csharp
[Fact]
public async Task MyService_WithRealDatabase_ProcessesCorrectly()
{
    // Arrange
    await using var container = new SqlServerBuilder().Build();
    await container.StartAsync();
    
    var service = new MyHostedService(container.GetConnectionString());
    
    // Act
    await service.StartAsync(CancellationToken.None);
    await Task.Delay(5000); // Let it run
    await service.StopAsync(CancellationToken.None);
    
    // Assert
    var result = await GetProcessedDataAsync(container.GetConnectionString());
    Assert.NotEmpty(result);
}
```

### How do I test logging?

Use a test sink or mock logger:

```csharp
var logOutput = new List<string>();
var logger = new LoggerConfiguration()
    .WriteTo.Sink(new DelegatingSink(e => logOutput.Add(e.RenderMessage())))
    .CreateLogger();

// Run service

Assert.Contains("Expected log message", logOutput);
```

---

## Performance Questions

### What's the memory overhead?

Each service instance has:
- One `ServiceProvider` (~few KB)
- Registered services in DI container
- Logging infrastructure

Typical overhead: < 1 MB per service for most scenarios.

### Can I run multiple instances?

Yes! You can run multiple instances of the same or different services:

```csharp
services.AddHostedService<WorkerService>();
services.AddHostedService<WorkerService>(); // Second instance
services.AddHostedService<DifferentService>();
```

Each gets its own isolated service collection.

### How does it perform under load?

Performance depends on your implementation. The framework overhead is minimal:
- Logging is asynchronous
- DI resolution is fast
- No polling or busy-waiting

Optimize your `ExecuteAsync` implementation for your workload.

---

## Deployment Questions

### How do I deploy as a Windows Service?

```csharp
var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // Add this
    .ConfigureServices(services =>
    {
        services.AddHostedService<MyService>();
    })
    .Build();

await host.RunAsync();
```

Install with:
```powershell
sc create MyService binPath="C:\path\to\myapp.exe"
```

### How do I deploy as a Linux daemon?

```csharp
var host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // Add this
    .ConfigureServices(services =>
    {
        services.AddHostedService<MyService>();
    })
    .Build();

await host.RunAsync();
```

Create a systemd service file:
```ini
[Unit]
Description=My Service

[Service]
Type=notify
ExecStart=/usr/local/bin/myapp

[Install]
WantedBy=multi-user.target
```

### How do I deploy to Docker?

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "MyService.dll"]
```

### How do I deploy to Kubernetes?

Create a Deployment:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: my-service
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: my-service
        image: myregistry/myservice:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
```

---

## Troubleshooting

### My service won't start

Check:
1. Exception in `RegisterDependencyInjection`?
2. Missing configuration values?
3. Network/database not accessible?
4. Check logs for startup errors

### My service won't stop gracefully

Ensure:
1. You're checking `stoppingToken.IsCancellationRequested`
2. Not blocking indefinitely
3. Timeout is sufficient
4. No deadlocks in cleanup code

### Logs aren't appearing

Verify:
1. Log level is set correctly
2. Sinks are configured
3. File permissions (for file sink)
4. Console output isn't redirected

### Dependencies aren't being resolved

Check:
1. Services are registered in `RegisterDependencyInjection`
2. Lifetimes are correct (Singleton, Scoped, Transient)
3. Using the right `ServiceProvider` (not a different instance)
4. No circular dependencies

---

## Contributing

### I found a bug, what should I do?

1. Check if it's already reported
2. Create a minimal reproduction
3. Open an issue with details
4. Include .NET version, OS, error messages

### I have a feature idea

Great! Please:
1. Check if it's already planned in the issues tab
2. Open a discussion to gather feedback
3. Consider if it fits the project goals

---

## More Questions?

- Check the [documentation](DOCUMENTATION_INDEX.md)
- Search [closed issues](../../issues?q=is%3Aissue+is%3Aclosed)
- Start a [discussion](../../discussions)
- Contact the maintainers

We're here to help!
