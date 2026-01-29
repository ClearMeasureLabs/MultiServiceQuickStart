# QuickHostedEndpoint API Reference

Complete API reference for the QuickHostedEndpoint library.

## BaseEndpointService

The main abstract class for creating NServiceBus endpoint hosted services.

**Namespace:** `QuickHostedEndpoint.Application`

**Inheritance:** `BaseHostedService` ? `BaseEndpointService`

### Properties

#### EndpointInstance

```csharp
protected IEndpointInstance EndpointInstance { get; }
```

Gets the NServiceBus endpoint instance. Only available after `StartAsync` has completed.

**Throws:** `InvalidOperationException` if accessed before the endpoint has started.

---

#### EndpointOptions

```csharp
protected virtual EndpointOptions EndpointOptions { get; }
```

Gets the endpoint configuration options. Override to customize endpoint behavior.

**Default:** New `EndpointOptions` instance with default values.

---

#### SqlPersistenceOptions

```csharp
protected virtual SqlPersistenceOptions? SqlPersistenceOptions { get; }
```

Gets the SQL persistence configuration options. Return `null` to use LearningPersistence.

**Default:** `null` (uses LearningPersistence)

---

#### EffectiveEndpointName

```csharp
protected string EffectiveEndpointName { get; }
```

Gets the effective endpoint name. Returns `EndpointOptions.EndpointName` if set, otherwise the class type name.

---

### Methods

#### ConfigureTransport (Abstract)

```csharp
protected abstract void ConfigureTransport(EndpointConfiguration endpointConfiguration);
```

Configures the message transport. **Must be implemented** by derived classes.

**Parameters:**
- `endpointConfiguration` - The NServiceBus endpoint configuration.

**Example:**
```csharp
protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
{
    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
    transport.ConnectionString("host=localhost");
}
```

---

#### ConfigureEndpoint

```csharp
protected virtual void ConfigureEndpoint(EndpointConfiguration endpointConfiguration);
```

Override to perform additional endpoint configuration before transport setup.

**Parameters:**
- `endpointConfiguration` - The NServiceBus endpoint configuration.

---

#### ConfigureEndpointAsync

```csharp
protected virtual Task ConfigureEndpointAsync(
    EndpointConfiguration endpointConfiguration, 
    CancellationToken cancellationToken);
```

Override to perform additional async endpoint configuration after all other configuration.

**Parameters:**
- `endpointConfiguration` - The NServiceBus endpoint configuration.
- `cancellationToken` - A cancellation token.

**Returns:** A task representing the async operation.

---

#### ConfigureSerialization

```csharp
protected virtual void ConfigureSerialization(EndpointConfiguration endpointConfiguration);
```

Configures the message serializer. Override to use a different serializer.

**Default:** Uses `SystemJsonSerializer`.

**Parameters:**
- `endpointConfiguration` - The NServiceBus endpoint configuration.

---

#### ConfigurePersistence

```csharp
protected virtual void ConfigurePersistence(EndpointConfiguration endpointConfiguration);
```

Configures persistence for sagas and subscriptions.

- If `SqlPersistenceOptions` is null, uses `LearningPersistence`.
- If `SqlPersistenceOptions` is provided, configures SQL Server persistence.

**Parameters:**
- `endpointConfiguration` - The NServiceBus endpoint configuration.

---

#### ConfigureRecoverability

```csharp
protected virtual void ConfigureRecoverability(EndpointConfiguration endpointConfiguration);
```

Configures the retry policy for failed messages.

**Default behavior:**
- Immediate retries: `EndpointOptions.ImmediateRetryCount`
- Delayed retries: `EndpointOptions.DelayedRetryCount`
- Time increase: `EndpointOptions.DelayedRetryTimeIncrease`

**Parameters:**
- `endpointConfiguration` - The NServiceBus endpoint configuration.

---

#### CreateDbConnection

```csharp
protected virtual DbConnection CreateDbConnection(string connectionString);
```

Creates a database connection for SQL persistence. Override to use a different database provider.

**Default:** Returns `SqlConnection` (SQL Server).

**Parameters:**
- `connectionString` - The connection string.

**Returns:** A `DbConnection` instance.

---

#### ExecuteAsync

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken);
```

The main execution loop. For NServiceBus endpoints, this keeps the service running until cancellation.

Override to add periodic tasks alongside message processing.

**Parameters:**
- `stoppingToken` - Token that signals shutdown.

---

#### OnStoppingAsync

```csharp
public override async Task OnStoppingAsync(CancellationToken cancellationToken);
```

Called when the hosted service is stopping. Stops the NServiceBus endpoint gracefully.

**Important:** Always call `base.OnStoppingAsync()` if you override this method.

**Parameters:**
- `cancellationToken` - A cancellation token.

---

## EndpointOptions

Configuration options for NServiceBus endpoint hosting.

**Namespace:** `QuickHostedEndpoint.Infrastructure.Configuration`

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EndpointName` | `string?` | `null` | Endpoint name (uses class name if null) |
| `EnableInstallers` | `bool` | `true` | Auto-create queues and tables on startup |
| `PurgeOnStartup` | `bool` | `false` | Purge queues on startup (dev only) |
| `ErrorQueue` | `string` | `"error"` | Queue for failed messages |
| `AuditQueue` | `string?` | `null` | Queue for audit messages (null = disabled) |
| `EnableMetrics` | `bool` | `false` | Enable NServiceBus metrics |
| `ImmediateRetryCount` | `int` | `3` | Number of immediate retries |
| `DelayedRetryCount` | `int` | `3` | Number of delayed retries |
| `DelayedRetryTimeIncrease` | `TimeSpan` | `10 seconds` | Time increase between delayed retries |
| `MaxConcurrency` | `int` | `1` | Maximum concurrent message processing |
| `EnableOutbox` | `bool` | `false` | Enable outbox for exactly-once processing |
| `OutboxCleanupBatchSize` | `int` | `100` | Outbox cleanup batch size |
| `OutboxTimeToKeepDeduplicationData` | `TimeSpan` | `7 days` | How long to keep outbox deduplication data |

---

## SqlPersistenceOptions

Configuration options for SQL Server persistence.

**Namespace:** `QuickHostedEndpoint.Infrastructure.Configuration`

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | `string?` | `null` | SQL Server connection string (required) |
| `Schema` | `string` | `"dbo"` | Database schema for persistence tables |
| `TablePrefix` | `string?` | `null` | Table name prefix (uses endpoint name if null) |
| `EnableSagaPersistence` | `bool` | `true` | Enable saga state persistence |
| `EnableSubscriptionStorage` | `bool` | `false` | Enable subscription storage |
| `SubscriptionCachePeriod` | `TimeSpan` | `5 seconds` | Subscription cache duration |

---

## EndpointConfigurationException

Exception thrown when there is an error configuring or starting the NServiceBus endpoint.

**Namespace:** `QuickHostedEndpoint.Core.Exceptions`

**Inheritance:** `Exception` ? `EndpointConfigurationException`

### Constructors

```csharp
public EndpointConfigurationException();
public EndpointConfigurationException(string message);
public EndpointConfigurationException(string message, Exception innerException);
```

---

## Inherited from BaseHostedService

`BaseEndpointService` inherits the following from `BaseHostedService`:

### Properties

| Property | Description |
|----------|-------------|
| `ServiceProvider` | Isolated service provider for this endpoint |
| `Logger` | Serilog logger instance |
| `Options` | Hosted service configuration options |

### Methods

| Method | Description |
|--------|-------------|
| `RegisterDependencyInjection(IServiceCollection)` | Register custom dependencies |
| `ConfigureLogging()` | Configure Serilog |
| `GetLoggingOptions()` | Provide logging options |
| `OnStartingAsync(CancellationToken)` | Pre-execution hook |

See [QuickHostedService API Reference](../QuickHostedService/API.md) for details.
