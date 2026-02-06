# Multi-Service Quick Start

An opinionated base implementation of `IHostedService` for building robust, production-ready background services in .NET with minimal boilerplate code.

## Overview

ClearHostedService provides a streamlined foundation for creating background services and hosted applications in .NET. It eliminates repetitive setup code by providing sensible defaults for logging, dependency injection, and application lifecycle management.

## Projects

| Project | Description | Package |
|---------|-------------|---------|
| **ClearHostedService** | Core library for building background services with Serilog logging and isolated DI | `ClearMeasure.HostedService` |
| **ClearHostedEndpoint** | Extension for hosting NServiceBus endpoints with SQL persistence support | `ClearMeasure.HostedEndpoint` |
| **ClearHostedEndpoint.SqlServerTransport** | SQL Server transport extensions with ambient transaction support | `ClearMeasure.HostedEndpoint.SqlServerTransport` |

## Key Features

### ClearHostedService

- ✅ **Opinionated Base Implementation**: Built on top of `IHostedService` with best practices baked in
- 📝 **Rich Logging**: Pre-configured Serilog integration with console, file, and ApplicationInsights support
- 💉 **Isolated Dependency Injection**: Each hosted service instance has its own `IServiceProvider`
- 🔄 **Lifecycle Hooks**: `OnStartingAsync` and `OnStoppingAsync` for initialization and cleanup
- ⚙️ **Configurable Options**: Control shutdown timeout, error handling, and service naming
- 🏗️ **Onion Architecture**: Clean separation of concerns following architectural best practices
- 🚀 **Production Ready**: Handles startup, shutdown, and error scenarios gracefully
- 🧪 **Testable**: Designed with unit testing and integration testing in mind

### ClearHostedEndpoint

- 📨 **NServiceBus Integration**: Seamless hosting of NServiceBus endpoints as background services
- 🗄️ **SQL Persistence**: Built-in SQL Server persistence for sagas and outbox
- 🔧 **Flexible Configuration**: Override transport, serialization, persistence, and recoverability settings
- 📊 **Endpoint Options**: Control concurrency, retries, error queues, and metrics
- 🔌 **Transport Agnostic**: Support for any NServiceBus transport (RabbitMQ, Azure Service Bus, SQL, MSMQ, etc.)
- 🎯 **Outbox Support**: Enable exactly-once processing with transactional outbox
- ⚡ **SQL Server Transport Extensions**: Ambient transaction support for SQL Server transport via `ClearHostedEndpoint.SqlServerTransport`

## Quick Start

### ClearHostedService - Background Service

Create a background service by inheriting from `ClearHostedService`:

```csharp
using ClearMeasure.HostedService;
using Microsoft.Extensions.DependencyInjection;

public class MyBackgroundService : ClearHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        // Register your application-specific dependencies
        services.AddScoped<IMyService, MyService>();
        services.AddSingleton<IMyRepository, MyRepository>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Your main service loop
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = ServiceProvider.CreateScope();
            var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
            
            await myService.DoWorkAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
    
    public override async Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Service is initializing...");
        // Perform startup validation, warm up caches, etc.
    }
    
    public override async Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("Service is shutting down...");
        // Flush queues, close connections, save state, etc.
    }
}
```

### ClearHostedEndpoint - NServiceBus Endpoint

Create an NServiceBus endpoint by inheriting from `ClearHostedEndpoint`:

```csharp
using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.Configuration;
using NServiceBus;

public class OrderProcessingEndpoint : ClearHostedEndpoint
{
    // Configure endpoint options
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "OrderProcessing",
        EnableInstallers = true,
        MaxConcurrency = 4,
        ImmediateRetryCount = 3,
        DelayedRetryCount = 3
    };

    // Configure SQL persistence for sagas
    protected override SqlPersistenceOptions SqlPersistenceOptions { get; } = new()
    {
        ConnectionString = "Server=localhost;Database=OrderDb;Integrated Security=true;",
        Schema = "nsb",
        EnableSagaPersistence = true
    };

    // Configure the message transport
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString("host=localhost");
        transport.UseConventionalRoutingTopology(QueueType.Quorum);
    }
    
    // Register message handlers and services
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddSingleton<IEmailService, EmailService>();
    }
}
```

### Using SQL Server Transport with Ambient Transactions

```csharp
using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.SqlServerTransport;
using NServiceBus;

public class PaymentEndpoint : ClearHostedEndpoint
{
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "PaymentProcessing"
    };

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        // Use SQL Server transport with ambient transaction support
        endpointConfiguration.UseSqlServerTransportWithAmbientTransaction(
            connectionString: "Server=localhost;Database=Transport;Integrated Security=true;",
            schema: "transport"
        );
    }
}
```

## Design Philosophy

ClearHostedService is designed around several core principles:

1. **Convention over Configuration**: Sensible defaults that work for most scenarios
2. **Isolation**: Each hosted service manages its own service collection and lifetime
3. **Observability**: Comprehensive logging and monitoring out of the box
4. **Maintainability**: Clean architecture principles for long-term maintainability
5. **Testability**: Designed with unit testing and integration testing in mind

## ClearHostedService Features in Detail

### Isolated Dependency Injection

Each `ClearHostedService` instance has its own isolated `IServiceProvider`, allowing:
- Multiple instances of the same service type with different configurations
- Independent service lifetimes per hosted service
- No DI container pollution between services

### Lifecycle Management

**Available lifecycle hooks:**
- `OnStartingAsync(CancellationToken)` - Called before `ExecuteAsync`, ideal for initialization
- `ExecuteAsync(CancellationToken)` - Your main service logic (abstract, must implement)
- `OnStoppingAsync(CancellationToken)` - Called during graceful shutdown, ideal for cleanup

**Graceful shutdown:**
- Configurable shutdown timeout (default: 30 seconds)
- Automatic cancellation token propagation
- Proper cleanup and disposal

### Logging Configuration

**Built-in Serilog integration with:**
- Console logging with customizable output templates
- File logging with rolling intervals
- ApplicationInsights integration (optional)
- Structured logging with enrichment (machine name, user, process ID, thread ID)

**Override logging behavior:**
```csharp
protected override void ConfigureLogging()
{
    // Fully customize Serilog configuration
}

// OR use simplified options
protected override LoggingOptions GetLoggingOptions()
{
    return new LoggingOptions
    {
        LogLevel = LogEventLevel.Debug,
        EnableConsoleLogging = true,
        EnableFileLogging = true,
        ApplicationInsightsConnectionString = "your-connection-string"
    };
}
```

### Customization Points

All virtual methods available for override:
- `RegisterDependencyInjection(IServiceCollection)` - Register your services
- `CreateServiceCollection()` - Provide a custom service collection
- `BuildServiceProviderAsync(IServiceCollection, CancellationToken)` - Custom service provider creation
- `ConfigureLogging()` - Full Serilog configuration control
- `GetLoggingOptions()` - Simplified logging configuration
- `OnStartingAsync(CancellationToken)` - Pre-execution initialization
- `OnStoppingAsync(CancellationToken)` - Pre-disposal cleanup

## ClearHostedEndpoint Features in Detail

### NServiceBus Integration

**Simplified endpoint hosting:**
- Automatic endpoint lifecycle management
- Seamless integration with ClearHostedService logging and DI
- Access to `EndpointInstance` for sending messages
- Convention-based endpoint naming

### Configuration Options

**EndpointOptions:**
- `EndpointName` - Endpoint name (defaults to class name)
- `EnableInstallers` - Auto-create queues/tables (default: true)
- `PurgeOnStartup` - Clear queue on startup (dev only, default: false)
- `ErrorQueue` - Error queue name (default: "error")
- `AuditQueue` - Audit queue name (optional)
- `MaxConcurrency` - Message processing concurrency (default: 1)
- `ImmediateRetryCount` - Immediate retry attempts (default: 3)
- `DelayedRetryCount` - Delayed retry attempts (default: 3)
- `DelayedRetryTimeIncrease` - Retry delay increment (default: 10 seconds)
- `EnableMetrics` - Enable NServiceBus metrics (default: false)
- `EnableOutbox` - Enable transactional outbox (default: false)
- `OutboxCleanupBatchSize` - Outbox cleanup size (default: 100)
- `OutboxTimeToKeepDeduplicationData` - Deduplication retention (default: 7 days)

**SqlPersistenceOptions:**
- `ConnectionString` - SQL connection string (required for SQL persistence)
- `Schema` - Database schema (default: "dbo")
- `TablePrefix` - Table prefix (defaults to endpoint name)
- `EnableSagaPersistence` - Enable saga storage (default: true)
- `EnableSubscriptionStorage` - Enable subscription storage (default: false)
- `SubscriptionCachePeriod` - Cache period (default: 5 seconds)

### Transport Configuration

**Supports all NServiceBus transports:**
- RabbitMQ
- Azure Service Bus
- Amazon SQS
- SQL Server (with ambient transaction support via extension package)
- MSMQ
- Learning Transport

**Abstract method to implement:**
```csharp
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    // Configure your chosen transport
}
```

### Customization Points

All virtual methods available for override:
- `ConfigureTransport(EndpointConfiguration)` - Configure message transport (required)
- `ConfigureEndpoint(EndpointConfiguration)` - Additional synchronous configuration
- `ConfigureEndpointAsync(EndpointConfiguration, CancellationToken)` - Async configuration
- `ConfigureSerialization(EndpointConfiguration)` - Message serialization (default: SystemTextJson)
- `ConfigurePersistence(EndpointConfiguration)` - Persistence configuration (default: SQL or Learning)
- `ConfigureRecoverability(EndpointConfiguration)` - Retry and error handling

### SQL Server Transport Extensions

**ClearHostedEndpoint.SqlServerTransport package provides:**
- `UseSqlServerTransportWithAmbientTransaction()` - Configure SQL transport with TransactionScope
- `StorageContext` - Access to the ambient database connection/transaction
- `StorageContextBehavior` - Pipeline behavior for connection management

**Example with ambient transactions:**
```csharp
public class MyHandler : IHandleMessages<MyMessage>
{
    private readonly StorageContext _storageContext;
    
    public MyHandler(StorageContext storageContext)
    {
        _storageContext = storageContext;
    }
    
    public async Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        // Use the same connection/transaction as NServiceBus
        using var command = _storageContext.Connection.CreateCommand();
        command.Transaction = _storageContext.Transaction;
        command.CommandText = "INSERT INTO Orders VALUES (@OrderId)";
        await command.ExecuteNonQueryAsync();
    }
}
```

## Building from Source

### Prerequisites

- .NET 10.0 SDK or later
- PowerShell 7+ (for build script)
- Bash (for Linux/macOS build script)

### Build Commands

The project includes cross-platform build scripts in the `src/` directory:

**Windows (PowerShell):**
```powershell
# Build with default settings (Release configuration)
.\src\build.ps1

# Build in Debug mode
.\src\build.ps1 -Configuration Debug

# Clean build
.\src\build.ps1 -Clean

# Build and create NuGet packages
.\src\build.ps1 -Pack

# Build without running tests
.\src\build.ps1 -SkipTests

# Custom package output directory
.\src\build.ps1 -Pack -PackageOutputPath "C:\packages"

# Verbose output
.\src\build.ps1 -VerboseOutput

# Combined example: Clean Release build with packages
.\src\build.ps1 -Clean -Pack -Configuration Release
```

**Windows (Command Prompt):**
```cmd
src\build.cmd
src\build.cmd -Configuration Debug -Clean
```

**Linux/macOS (Bash):**
```bash
./src/build.sh
./src/build.sh --configuration Debug --clean
./src/build.sh --pack
```

### Build Script Features

The build script (`build.ps1`) provides:
- ✅ **Prerequisites Check** - Verifies .NET SDK installation
- 🧹 **Clean** - Removes bin/obj directories
- 📦 **NuGet Restore** - Restores package dependencies
- 🔨 **Build** - Compiles all projects in dependency order
- 🧪 **Test** - Runs all unit tests with xUnit
- 📦 **Pack** - Creates NuGet packages for library projects
- 📊 **Summary** - Shows build duration and results

**Projects built in order:**
1. ClearMeasure.HostedService
2. ClearMeasure.HostedEndpoint
3. ClearMeasure.HostedEndpoint.SqlServerTransport
4. ClearHostedService.Tests
5. ClearHostedEndpoint.Tests

**Package output:**
- Default location: `src/artifacts/packages/`
- Includes `.nupkg` files
- Includes symbol packages (`.snupkg`) for Release builds

### Build Script Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-Configuration` | Build configuration (Debug/Release) | Release |
| `-Clean` | Clean before build | false |
| `-SkipTests` | Skip running tests | false |
| `-Pack` | Create NuGet packages | false |
| `-PackageOutputPath` | Package output directory | ./artifacts/packages |
| `-VerboseOutput` | Enable verbose MSBuild output | false |

### Manual Build

You can also build manually using `dotnet` CLI:

```bash
# Restore packages
dotnet restore src/ClearHostedService.sln

# Build solution
dotnet build src/ClearHostedService.sln --configuration Release

# Run tests
dotnet test src/ClearHostedService.sln --configuration Release

# Create packages
dotnet pack src/ClearHostedService/ClearMeasure.HostedService.csproj -c Release -o ./packages
dotnet pack src/ClearHostedEndpoint/ClearMeasure.HostedEndpoint.csproj -c Release -o ./packages
dotnet pack src/ClearHostedEndpoint.SqlServerTransport/ClearMeasure.HostedEndpoint.SqlServerTransport.csproj -c Release -o ./packages
```

## Architecture

This library follows **Onion Architecture** principles with clear separation between:

- **Core Domain**: Business logic and domain models (Interfaces, Exceptions)
- **Application Services**: Orchestration and use cases (ClearHostedService, ClearHostedEndpoint)
- **Infrastructure**: Logging, external dependencies, and cross-cutting concerns (Configuration, Extensions)
- **Presentation**: Hosted service entry points

For detailed architecture documentation, see [ARCHITECTURE.md](/Docs/ClearHostedService/ARCHITECTURE.md).

## Installation

### From NuGet (when published)

```bash
# ClearHostedService - Core library
dotnet add package ClearMeasure.HostedService

# ClearHostedEndpoint - NServiceBus integration
dotnet add package ClearMeasure.HostedEndpoint

# ClearHostedEndpoint.SqlServerTransport - SQL Server transport extensions
dotnet add package ClearMeasure.HostedEndpoint.SqlServerTransport
```

### From Local Build

After building with `-Pack`:

```bash
# Add local package source
dotnet nuget add source "D:\cm-internal\MultiServiceQuickStart\src\artifacts\packages" --name LocalPackages

# Install packages
dotnet add package ClearMeasure.HostedService --source LocalPackages
dotnet add package ClearMeasure.HostedEndpoint --source LocalPackages
dotnet add package ClearMeasure.HostedEndpoint.SqlServerTransport --source LocalPackages
```

## Use Cases

ClearHostedService is ideal for:

- Background workers and data processors
- Scheduled tasks and recurring jobs
- Message queue consumers
- Monitoring and health check services
- Microservices and standalone applications
- Console applications requiring DI and logging

## Project Structure

```
ClearHostedService/
??? Core/               # Domain models and core abstractions
??? Application/        # ClearHostedService implementation
??? Infrastructure/     # Logging, configuration, external services
??? Extensions/         # Helper methods and service registration

ClearHostedEndpoint/
??? Core/               # Endpoint exceptions
??? Application/        # ClearHostedEndpoint implementation
??? Infrastructure/     # EndpointOptions, SqlPersistenceOptions
```

## Requirements

- .NET 10.0 or later
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Serilog (for logging)
- ApplicationInsights (optional, for monitoring)
- NServiceBus 9.x (for ClearHostedEndpoint)

## Documentation

### ClearHostedService
- [Architecture Overview](Docs/ClearHostedService/ARCHITECTURE.md)
- [Usage Guide](Docs/ClearHostedService/USAGE.md)
- [API Reference](Docs/ClearHostedService/API.md)

### ClearHostedEndpoint
- [README](Docs/ClearHostedEndpoint/README.md)
- [Architecture Overview](Docs/ClearHostedEndpoint/ARCHITECTURE.md)
- [Usage Guide](Docs/ClearHostedEndpoint/USAGE.md)
- [API Reference](Docs/ClearHostedEndpoint/API.md)

### Examples
- [SimpleWorker](src/examples/SimpleWorker) - Basic background worker
- [DataProcessorService](src/examples/DataProcessorService) - DI and batch processing
- [ScheduledTaskService](src/examples/ScheduledTaskService) - Time-based scheduling
- [NServiceBusEndpoint](src/examples/NServiceBusEndpoint) - NServiceBus message endpoint
- [NServiceBusWebApp](src/examples/NServiceBusWebApp) - Web app sending messages to endpoint

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](Docs/ClearHostedService/CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## Quick Reference

### Common Tasks

| Task | Command/Code |
|------|--------------|
| **Build the solution** | `.\src\build.ps1` |
| **Clean build** | `.\src\build.ps1 -Clean` |
| **Build packages** | `.\src\build.ps1 -Pack` |
| **Run tests** | `.\src\build.ps1` (tests run by default) |
| **Skip tests** | `.\src\build.ps1 -SkipTests` |
| **Debug build** | `.\src\build.ps1 -Configuration Debug` |
| **Create hosted service** | Inherit from `ClearHostedService` |
| **Create NServiceBus endpoint** | Inherit from `ClearHostedEndpoint` |
| **Access logger** | Use `Logger` property |
| **Access DI container** | Use `ServiceProvider` property |
| **Send NServiceBus message** | Use `EndpointInstance.Send()` or `.Publish()` |

### Key Classes and Interfaces

| Type | Purpose |
|------|---------|
| `ClearHostedService` | Base class for background services |
| `ClearHostedEndpoint` | Base class for NServiceBus endpoints |
| `IHostedServiceLifecycle` | Interface for lifecycle hooks |
| `HostedServiceOptions` | Configuration for ClearHostedService |
| `EndpointOptions` | Configuration for ClearHostedEndpoint |
| `SqlPersistenceOptions` | SQL persistence configuration |
| `LoggingOptions` | Serilog logging configuration |
| `StorageContext` | SQL Server transport ambient transaction context |

### Virtual Methods to Override

**ClearHostedService:**
- `RegisterDependencyInjection(IServiceCollection)` - Register services
- `ConfigureLogging()` - Configure Serilog
- `GetLoggingOptions()` - Simple logging config
- `OnStartingAsync(CancellationToken)` - Initialization hook
- `OnStoppingAsync(CancellationToken)` - Cleanup hook
- `ExecuteAsync(CancellationToken)` - Main logic (abstract)

**ClearHostedEndpoint:**
- All ClearHostedService methods, plus:
- `ConfigureTransport(EndpointConfiguration)` - Configure transport (abstract)
- `ConfigureEndpoint(EndpointConfiguration)` - Additional config
- `ConfigureSerialization(EndpointConfiguration)` - Message serialization
- `ConfigurePersistence(EndpointConfiguration)` - Persistence config
- `ConfigureRecoverability(EndpointConfiguration)` - Retry config

## License

[Specify License]

## Support

For issues, questions, or contributions, please [open an issue](../../issues) or submit a pull request.
