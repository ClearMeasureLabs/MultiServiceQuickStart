# GitHub Copilot Instructions for QuickHostedService

This file provides instructions and context for GitHub Copilot when working with the QuickHostedService project.

## Project Overview

**QuickHostedService** is an opinionated base implementation of `IHostedService` for building robust, production-ready background services in .NET with minimal boilerplate code.

### Key Characteristics
- **Target Framework**: .NET 10.0
- **C# Version**: 14.0
- **Architecture**: Onion Architecture
- **Logging**: Serilog with structured logging
- **DI**: Isolated service collections per hosted service instance

## Architecture Principles

### Onion Architecture Layers
1. **Core** (`QuickHostedService/Core/`) - Pure abstractions, no dependencies
   - Interfaces: `IHostedServiceLifecycle`, `IServiceRegistration`
   - Exceptions: `HostedServiceException`, `ServiceRegistrationException`

2. **Application** (`QuickHostedService/Application/`) - Business logic
   - `BaseHostedService` - Main abstract base class

3. **Infrastructure** (`QuickHostedService/Infrastructure/`) - External concerns
   - Configuration: `LoggingOptions`, `HostedServiceOptions`
   - Logging setup with Serilog

### Key Design Decisions
- **Isolated Service Collections**: Each `BaseHostedService` instance has its own `IServiceProvider`
- **Public Lifecycle Methods**: `OnStartingAsync` and `OnStoppingAsync` are `public virtual`
- **Abstract ExecuteAsync**: Must be implemented by derived classes
- **Graceful Shutdown**: 30-second default timeout, configurable

## Coding Standards

### Naming Conventions
- **Classes/Methods/Properties**: PascalCase
- **Private fields**: `_camelCase` with underscore prefix
- **Interfaces**: Start with 'I' (e.g., `IHostedServiceLifecycle`)
- **Async methods**: End with 'Async'

### Code Style
```csharp
// Good example
public class MyService : BaseHostedService
{
    private readonly ILogger<MyService> _logger;
    
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyServiceImpl>();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

### XML Documentation
- All public APIs require XML documentation
- Use `<summary>`, `<param>`, `<returns>`, `<exception>` tags
- Keep descriptions concise but clear

### Lifecycle Method Overrides
When overriding lifecycle methods, they **must be** `public override`:

```csharp
// Correct
public override async Task OnStartingAsync(CancellationToken cancellationToken)
{
    // Initialization logic
}

public override Task OnStoppingAsync(CancellationToken cancellationToken)
{
    // Cleanup logic  
}

// Incorrect - Do not use 'protected'
protected override Task OnStartingAsync(CancellationToken cancellationToken) // ? Wrong!
```

## Common Patterns

### Basic Hosted Service
```csharp
public class MyWorker : BaseHostedService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Logger.Information("Working...");
            await Task.Delay(5000, stoppingToken);
        }
    }
}
```

### With Dependency Injection
```csharp
public class DataProcessor : BaseHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddSingleton<IDataRepository, DataRepository>();
        services.AddScoped<IDataProcessor, Processor>();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = ServiceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();
        await processor.ProcessAsync(stoppingToken);
    }
}
```

### With Lifecycle Hooks
```csharp
public class ServiceWithHooks : BaseHostedService
{
    public override async Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Initializing...");
        // Validation, warm-up, etc.
    }
    
    public override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Cleaning up...");
        // Flush queues, close connections, etc.
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Main logic
    }
}
```

## Testing Guidelines

### Unit Test Pattern
```csharp
[Fact]
public async Task ServiceName_Scenario_ExpectedResult()
{
    // Arrange
    var service = new TestService();
    
    // Act
    await service.StartAsync(CancellationToken.None);
    await Task.Delay(100);
    await service.StopAsync(CancellationToken.None);
    
    // Assert
    service.SomeProperty.Should().BeTrue();
}
```

### Testing with Dependencies
```csharp
[Fact]
public async Task ProcessData_WithValidInput_ProcessesSuccessfully()
{
    // Arrange
    var mockRepo = new Mock<IDataRepository>();
    mockRepo.Setup(r => r.GetDataAsync()).ReturnsAsync(testData);
    
    // Create service and inject mocks through DI
    var service = new TestService();
    // ... register mocks in DI
    
    // Act & Assert
}
```

## Documentation Structure

All documentation is located in `Docs/QuickHostedService/`:

- **README.md** - Project overview and quick start
- **ARCHITECTURE.md** - Detailed architecture and design
- **API.md** - Complete API reference
- **USAGE.md** - Usage examples and patterns
- **DESIGN_DECISIONS.md** - Rationale for key decisions
- **FAQ.md** - Common questions and troubleshooting
- **CONTRIBUTING.md** - Contribution guidelines
- **CHANGELOG.md** - Version history
- **DOCUMENTATION_INDEX.md** - Documentation navigation
- **PROJECT_SUMMARY.md** - Project implementation summary
- **IMPLEMENTATION_COMPLETE.md** - Implementation completion status

### Updating Documentation
When making changes to the library:
1. Update relevant API docs in `API.md`
2. Add usage examples to `USAGE.md`
3. Document architecture changes in `ARCHITECTURE.md`
4. Update `CHANGELOG.md` with version notes
5. Update this `Copilot.md` if patterns change

## Examples

Example projects are in `examples/`:

1. **SimpleWorker** - Basic continuous operation
2. **DataProcessorService** - DI, scoped services, batch processing
3. **ScheduledTaskService** - Time-based scheduling

When creating new examples:
- Follow existing structure
- Include comprehensive README.md
- Demonstrate specific patterns
- Keep code simple and educational

## Common Mistakes to Avoid

### ? Don't Do This
```csharp
// Wrong: Using protected instead of public for lifecycle methods
protected override Task OnStartingAsync(CancellationToken cancellationToken)

// Wrong: Not checking cancellation token
while (true) // Will never stop gracefully
{
    await DoWork();
}

// Wrong: Not using scopes for scoped services
var service = ServiceProvider.GetRequiredService<IScopedService>(); // Memory leak!

// Wrong: Blocking operations without cancellation
Thread.Sleep(5000); // Use await Task.Delay(5000, cancellationToken) instead
```

### ? Do This Instead
```csharp
// Correct: Public override for lifecycle methods
public override Task OnStartingAsync(CancellationToken cancellationToken)

// Correct: Always check cancellation token
while (!stoppingToken.IsCancellationRequested)
{
    await DoWork(stoppingToken);
}

// Correct: Create scope for scoped services
using var scope = ServiceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IScopedService>();

// Correct: Async delays with cancellation
await Task.Delay(5000, stoppingToken);
```

## Build and Test Commands

```bash
# Build entire solution
dotnet build QuickHostedService.sln

# Run all tests
dotnet test QuickHostedService.sln

# Run specific example
dotnet run --project examples/SimpleWorker/SimpleWorker.csproj
dotnet run --project examples/DataProcessorService/DataProcessorService.csproj
dotnet run --project examples/ScheduledTaskService/ScheduledTaskService.csproj

# Pack for NuGet
dotnet pack QuickHostedService/QuickHostedService.csproj -c Release
```

## Quick Reference

### BaseHostedService Members

| Member | Type | Description |
|--------|------|-------------|
| `ServiceProvider` | Property | Isolated service provider for this instance |
| `Logger` | Property | Serilog logger instance |
| `Options` | Property | Hosted service configuration options |
| `RegisterDependencyInjection(services)` | Virtual Method | Register DI dependencies |
| `ConfigureLogging()` | Virtual Method | Configure Serilog |
| `GetLoggingOptions()` | Virtual Method | Provide logging options |
| `OnStartingAsync(token)` | Public Virtual Method | Pre-execution initialization |
| `OnStoppingAsync(token)` | Public Virtual Method | Pre-disposal cleanup |
| `ExecuteAsync(token)` | Abstract Method | Main service logic (required) |

### When to Use Each Method

- **RegisterDependencyInjection**: Register your services (repositories, processors, etc.)
- **ConfigureLogging**: Customize log sinks, levels, formatters
- **GetLoggingOptions**: Provide logging configuration (simpler than ConfigureLogging)
- **OnStartingAsync**: Validate config, warm up caches, perform health checks
- **OnStoppingAsync**: Flush queues, close connections, save state
- **ExecuteAsync**: Your main service loop or one-time execution

## Support and Resources

- **Documentation**: See `Docs/QuickHostedService/` for comprehensive guides
- **Examples**: Check `examples/` for working implementations
- **Tests**: See `QuickHostedService.Tests/` for testing patterns

---

**Last Updated**: 2025-01-29  
**Version**: 1.0.0  
**Maintainer**: QuickHostedService Contributors
