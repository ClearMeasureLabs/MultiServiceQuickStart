# Examples

This directory contains example implementations demonstrating how to use ClearHostedService in various scenarios.

## Available Examples

### 1. SimpleWorker
A basic background worker that performs periodic tasks.

**Key Concepts:**
- Basic `ClearHostedService` implementation
- Simple periodic execution
- Cancellation token handling

**Use Case:**
- Health checks
- Periodic data cleanup
- Simple monitoring tasks

---

### 2. DataProcessorService
A data processing service with dependency injection.

**Key Concepts:**
- Custom dependency registration
- Scoped service usage
- Database operations
- Structured logging

**Use Case:**
- ETL processes
- Batch data processing
- Report generation

---

### 3. MessageQueueConsumer
A message queue consumer using RabbitMQ.

**Key Concepts:**
- Message queue integration
- Async enumerable consumption
- Error handling and retry logic
- Graceful connection management

**Use Case:**
- Event-driven architectures
- Microservices communication
- Asynchronous task processing

---

### 4. ScheduledTaskService
A service that executes tasks on a schedule.

**Key Concepts:**
- Time-based scheduling
- Cron-like functionality
- Task execution timing

**Use Case:**
- Daily reports
- Scheduled maintenance
- Time-triggered operations

---

### 5. ResilientService
A service with retry and circuit breaker patterns.

**Key Concepts:**
- Retry logic with exponential backoff
- Circuit breaker pattern
- Resilience strategies

**Use Case:**
- External API calls
- Unreliable network operations
- Fault-tolerant processing

---

### 6. MultiTenantService
A multi-tenant background service.

**Key Concepts:**
- Tenant isolation
- Parallel processing
- Per-tenant configuration

**Use Case:**
- SaaS applications
- Multi-customer data processing
- Isolated tenant operations

---

### 7. ApplicationInsightsService
A service with comprehensive telemetry.

**Key Concepts:**
- Custom metrics
- Dependency tracking
- Performance monitoring
- Custom events

**Use Case:**
- Production monitoring
- Performance analysis
- Business metrics tracking

---

## Running the Examples

Each example is a standalone console application. To run an example:

```bash
cd examples/[ExampleName]
dotnet run
```

## Example Structure

Each example follows this structure:

```
ExampleName/
??? Program.cs                 # Entry point and host configuration
??? ExampleService.cs          # The hosted service implementation
??? Services/                  # Supporting services and interfaces
??? Models/                    # Domain models
??? appsettings.json          # Configuration
??? README.md                 # Example-specific documentation
```

## Creating Your Own Service

Use these examples as templates:

1. Choose the example closest to your use case
2. Copy the project structure
3. Modify `RegisterDependencyInjection` for your dependencies
4. Implement `ExecuteAsync` with your business logic
5. Add lifecycle hooks (`OnStartingAsync`, `OnStoppingAsync`) as needed

## Common Patterns

### Pattern 1: Continuous Loop with Delay

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await DoWorkAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
    }
}
```

### Pattern 2: Event-Driven Processing

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await foreach (var message in _queue.ConsumeAsync(stoppingToken))
    {
        await ProcessMessageAsync(message, stoppingToken);
    }
}
```

### Pattern 3: One-Time Execution

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await DoWorkOnceAsync(stoppingToken);
    // Service will stop after completion
}
```

### Pattern 4: Scheduled Execution

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var nextRun = CalculateNextRunTime();
        var delay = nextRun - DateTime.Now;
        
        await Task.Delay(delay, stoppingToken);
        
        if (!stoppingToken.IsCancellationRequested)
        {
            await ExecuteScheduledWorkAsync(stoppingToken);
        }
    }
}
```

## Testing Examples

Each example includes unit tests demonstrating:

- Service startup and shutdown
- Dependency injection
- Logging verification
- Error handling
- Cancellation handling

To run example tests:

```bash
cd examples/[ExampleName].Tests
dotnet test
```

