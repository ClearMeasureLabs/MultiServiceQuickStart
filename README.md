# QuickHostedService

An opinionated base implementation of `IHostedService` for building robust, production-ready background services in .NET with minimal boilerplate code.

## Overview

QuickHostedService provides a streamlined foundation for creating background services and hosted applications in .NET. It eliminates repetitive setup code by providing sensible defaults for logging, dependency injection, and application lifecycle management.

## Projects

| Project | Description |
|---------|-------------|
| **QuickHostedService** | Core library for building background services with Serilog logging and isolated DI |
| **QuickHostedEndpoint** | Extension for hosting NServiceBus endpoints with SQL persistence support |

## Key Features

- **Opinionated Base Implementation**: Built on top of `IHostedService` with best practices baked in
- **Rich Logging**: Pre-configured Serilog integration with ApplicationInsights support
- **Dependency Injection**: Full support for Microsoft.Extensions.DependencyInjection with isolated service collections
- **Extensibility**: Easy-to-override methods for customizing application behavior
- **Onion Architecture**: Clean separation of concerns following architectural best practices
- **Production Ready**: Handles startup, shutdown, and error scenarios gracefully
- **NServiceBus Integration**: QuickHostedEndpoint provides seamless NServiceBus endpoint hosting

## Quick Start

### Background Service (QuickHostedService)

```csharp
public class MyBackgroundService : BaseHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        // Register your application-specific dependencies
        services.AddScoped<IMyService, MyService>();
        services.AddSingleton<IMyRepository, MyRepository>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Your application logic here
        var myService = ServiceProvider.GetRequiredService<IMyService>();
        await myService.DoWorkAsync(stoppingToken);
    }
}
```

### NServiceBus Endpoint (QuickHostedEndpoint)

```csharp
public class OrderProcessingEndpoint : BaseEndpointService
{
    protected override EndpointOptions EndpointOptions { get; } = new()
    {
        EndpointName = "OrderProcessing",
        EnableInstallers = true,
        MaxConcurrency = 4
    };

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString("host=localhost");
    }
}
```

## Design Philosophy

QuickHostedService is designed around several core principles:

1. **Convention over Configuration**: Sensible defaults that work for most scenarios
2. **Isolation**: Each hosted service manages its own service collection and lifetime
3. **Observability**: Comprehensive logging and monitoring out of the box
4. **Maintainability**: Clean architecture principles for long-term maintainability
5. **Testability**: Designed with unit testing and integration testing in mind

## Architecture

This library follows **Onion Architecture** principles with clear separation between:

- **Core Domain**: Business logic and domain models
- **Application Services**: Orchestration and use cases
- **Infrastructure**: Logging, external dependencies, and cross-cutting concerns
- **Presentation**: Hosted service entry points

For detailed architecture documentation, see [ARCHITECTURE.md](/Docs/QuickHostedService/ARCHITECTURE.md).

## Use Cases

QuickHostedService is ideal for:

- Background workers and data processors
- Scheduled tasks and recurring jobs
- Message queue consumers
- Monitoring and health check services
- Microservices and standalone applications
- Console applications requiring DI and logging

## Project Structure

```
QuickHostedService/
??? Core/               # Domain models and core abstractions
??? Application/        # Base hosted service implementation
??? Infrastructure/     # Logging, configuration, external services
??? Extensions/         # Helper methods and service registration

QuickHostedEndpoint/
??? Core/               # Endpoint exceptions
??? Application/        # BaseEndpointService implementation
??? Infrastructure/     # EndpointOptions, SqlPersistenceOptions
```

## Requirements

- .NET 10.0 or later
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Serilog (for logging)
- ApplicationInsights (optional, for monitoring)
- NServiceBus 9.x (for QuickHostedEndpoint)

## Documentation

### QuickHostedService
- [Architecture Overview](Docs/QuickHostedService/ARCHITECTURE.md)
- [Usage Guide](Docs/QuickHostedService/USAGE.md)
- [API Reference](Docs/QuickHostedService/API.md)

### QuickHostedEndpoint
- [README](Docs/QuickHostedEndpoint/README.md)
- [Architecture Overview](Docs/QuickHostedEndpoint/ARCHITECTURE.md)
- [Usage Guide](Docs/QuickHostedEndpoint/USAGE.md)
- [API Reference](Docs/QuickHostedEndpoint/API.md)

### Examples
- [SimpleWorker](src/examples/SimpleWorker) - Basic background worker
- [DataProcessorService](src/examples/DataProcessorService) - DI and batch processing
- [ScheduledTaskService](src/examples/ScheduledTaskService) - Time-based scheduling
- [NServiceBusEndpoint](src/examples/NServiceBusEndpoint) - NServiceBus message endpoint
- [NServiceBusWebApp](src/examples/NServiceBusWebApp) - Web app sending messages to endpoint

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](Docs/QuickHostedService/CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

[Specify License]

## Support

For issues, questions, or contributions, please [open an issue](../../issues) or submit a pull request.
