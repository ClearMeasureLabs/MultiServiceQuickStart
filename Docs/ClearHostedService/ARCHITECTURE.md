# Architecture Documentation

## Overview

ClearHostedService is built following **Onion Architecture** principles, ensuring a clean separation of concerns and making the codebase maintainable, testable, and extensible.

## Architectural Principles

### Onion Architecture

The library is structured in concentric layers, with dependencies pointing inward:

```
???????????????????????????????????????????
?     Infrastructure Layer               ?  ? Logging, External Services
???????????????????????????????????????????
?     Application Layer                  ?  ? ClearHostedService, Orchestration
???????????????????????????????????????????
?     Core/Domain Layer                  ?  ? Abstractions, Interfaces
???????????????????????????????????????????
```

**Key Rules:**
- Inner layers know nothing about outer layers
- Dependencies flow inward only
- Domain layer has no external dependencies
- Infrastructure is replaceable

### Dependency Inversion

All dependencies are abstracted through interfaces, allowing:
- Easy testing with mocks
- Swapping implementations without changing core logic
- Loose coupling between components

## Component Architecture

### 1. Core Layer

**Responsibility**: Define domain abstractions and contracts

**Components:**
- `IHostedServiceLifecycle`: Lifecycle hooks for hosted services
- `IServiceRegistration`: Contract for dependency registration
- `ILoggingConfiguration`: Logging configuration abstraction

**Characteristics:**
- No external dependencies
- Pure interfaces and abstractions
- Framework-agnostic where possible

### 2. Application Layer

**Responsibility**: Implement base hosted service orchestration

**Components:**
- `ClearHostedService`: Main base class for all hosted services
  - Implements `IHostedService`
  - Manages service collection lifecycle
  - Handles startup and shutdown sequences
  - Provides extension points for child classes

**Key Features:**
- Service collection isolation per hosted service instance
- Lifecycle management (StartAsync, StopAsync, ExecuteAsync)
- Graceful shutdown handling
- Error handling and recovery

**Extension Points:**
```csharp
protected virtual void RegisterDependencyInjection(IServiceCollection services)
protected virtual void ConfigureLogging(ILoggingBuilder builder)
protected virtual Task OnStartingAsync(CancellationToken cancellationToken)
protected virtual Task OnStoppingAsync(CancellationToken cancellationToken)
protected abstract Task ExecuteAsync(CancellationToken stoppingToken)
```

### 3. Infrastructure Layer

**Responsibility**: Provide concrete implementations for cross-cutting concerns

**Components:**

#### Logging Infrastructure
- `SerilogConfiguration`: Serilog setup and configuration
- `ApplicationInsightsConfiguration`: ApplicationInsights integration
- Default sinks: Console, File, ApplicationInsights
- Structured logging support

#### Service Registration
- `ServiceCollectionExtensions`: Helper methods for DI registration
- Common service registrations (HTTP clients, configurations, etc.)

## Dependency Injection Strategy

### Isolated Service Collections

Each `ClearHostedService` instance maintains its own `ServiceCollection`:

```
???????????????????????????????????
?  HostedService Instance A       ?
?  ??? ServiceProvider A          ?
?  ??? Logger A                   ?
?  ??? Dependencies A             ?
???????????????????????????????????

???????????????????????????????????
?  HostedService Instance B       ?
?  ??? ServiceProvider B          ?
?  ??? Logger B                   ?
?  ??? Dependencies B             ?
???????????????????????????????????
```

**Benefits:**
- No cross-contamination between services
- Independent lifecycle management
- Easier to reason about scope and lifetime
- Simpler testing

**Considerations:**
- Slightly higher memory overhead
- Cannot share scoped instances between hosted services
- Each service is truly independent

### Service Lifetime Guidelines

- **Singleton**: Use for stateless services, caches, shared resources
- **Scoped**: Use for request-scoped operations (though less common in background services)
- **Transient**: Use for lightweight, stateless operations

## Logging Architecture

### Serilog Integration

ClearHostedService uses Serilog as the primary logging framework:

**Default Configuration:**
- Minimum Level: Information
- Console sink with structured output
- File sink with rolling intervals (daily)
- ApplicationInsights sink (when configured)

**Log Enrichment:**
- Machine name
- Environment name
- Application name/version
- Thread ID
- Process ID

**Structured Logging:**
```csharp
logger.Information("Processing {RecordCount} records from {Source}", count, sourceName);
```

### ApplicationInsights Integration

Optional telemetry for production environments:

- Request tracking
- Dependency tracking
- Exception tracking
- Custom metrics and events
- Performance counters

## Lifecycle Management

### Startup Sequence

```
1. Constructor called
   ?
2. StartAsync() called by host
   ?
3. Configure Logging
   ?
4. Build Service Collection
   ?
5. Call RegisterDependencyInjection() (override point)
   ?
6. Build ServiceProvider
   ?
7. Call OnStartingAsync() (override point)
   ?
8. Start ExecuteAsync() on background thread
   ?
9. Return control to host
```

### Shutdown Sequence

```
1. StopAsync() called by host
   ?
2. Set cancellation token
   ?
3. Wait for ExecuteAsync() to complete (with timeout)
   ?
4. Call OnStoppingAsync() (override point)
   ?
5. Dispose ServiceProvider
   ?
6. Dispose resources
   ?
7. Return control to host
```

## Error Handling Strategy

### Startup Errors
- Logged with full context
- Propagated to host for handling
- Service fails to start

### Runtime Errors (in ExecuteAsync)
- Logged with stack trace
- Can be caught and handled by child implementations
- Can implement retry logic if needed

### Shutdown Errors
- Logged but don't prevent shutdown
- Resources still cleaned up
- Timeout enforced to prevent hanging

## Extensibility Points

### For Library Consumers

Developers using ClearHostedService can extend:

1. **Dependency Registration**: Override `RegisterDependencyInjection()`
2. **Logging Configuration**: Override `ConfigureLogging()`
3. **Startup Logic**: Override `OnStartingAsync()`
4. **Shutdown Logic**: Override `OnStoppingAsync()`
5. **Main Execution**: Implement `ExecuteAsync()`

### For Library Maintainers

Future extensions can add:

1. **Health Checks**: Built-in health check support
2. **Metrics**: Performance and business metrics
3. **Configuration**: File-based configuration support
4. **Middleware Pipeline**: Request processing pipeline
5. **Event Bus**: Internal event publishing/subscribing

## Testing Strategy

### Unit Testing

```csharp
public class MyServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ProcessesRecords_Successfully()
    {
        // Arrange
        var mockService = new Mock<IMyService>();
        var sut = new MyHostedService();
        
        // Override DI for testing
        sut.RegisterDependencyInjection(services => 
        {
            services.AddSingleton(mockService.Object);
        });
        
        // Act
        await sut.StartAsync(CancellationToken.None);
        
        // Assert
        mockService.Verify(x => x.DoWorkAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Integration Testing

- Test with real dependencies
- Use test containers for external services
- Verify logging output
- Test graceful shutdown

## Performance Considerations

1. **Service Provider Creation**: Minimal overhead, done once at startup
2. **Logging**: Asynchronous sinks for non-blocking writes
3. **Memory**: Isolated service collections use more memory but improve isolation
4. **Shutdown**: Configurable timeout to balance graceful shutdown vs. responsiveness

## Security Considerations

1. **Secrets Management**: Use configuration providers, not hardcoded values
2. **Logging**: Avoid logging sensitive data
3. **Dependencies**: Keep packages up to date
4. **Isolation**: Service isolation prevents cross-service contamination

## Future Architecture Enhancements

1. **Plugin System**: Load hosted services dynamically
2. **Configuration Validation**: Validate settings at startup
3. **Circuit Breaker**: Built-in resilience patterns
4. **Distributed Tracing**: OpenTelemetry integration
5. **gRPC Health Checks**: Standard health check protocol
