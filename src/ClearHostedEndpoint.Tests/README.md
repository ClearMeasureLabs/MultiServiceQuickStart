# ClearHostedEndpoint.Tests

Comprehensive unit test suite for the ClearHostedEndpoint library.

## Test Coverage

This test project provides extensive coverage of the ClearHostedEndpoint functionality:

### Configuration Tests

#### EndpointOptionsTests
- Tests all default values for endpoint configuration
- Validates property setters and getters
- Tests complex configuration scenarios
- Covers edge cases for retry counts, concurrency, and timeouts

#### SqlPersistenceOptionsTests
- Tests SQL persistence configuration options
- Validates connection string handling
- Tests schema and table prefix configuration
- Covers saga and subscription storage options

### Exception Tests

#### EndpointConfigurationExceptionTests
- Tests all exception constructors
- Validates exception inheritance
- Tests exception throwing and catching
- Covers message and inner exception handling

### Core Functionality Tests

#### ClearHostedEndpointTests
- Tests endpoint lifecycle (start/stop)
- Validates configuration method calls
- Tests endpoint naming (custom vs. type-based)
- Validates error handling during startup
- Tests resource cleanup and disposal

#### EndpointLifecycleTests
- Tests complete lifecycle scenarios
- Validates concurrent endpoint execution
- Tests graceful shutdown behavior
- Covers multiple start/stop scenarios
- Tests disposal in various states

### Advanced Configuration Tests

#### EndpointOptionsConfigurationTests
- Tests endpoint configuration with various options
- Validates installer configuration
- Tests error and audit queue configuration
- Tests concurrency and retry settings
- Validates outbox configuration

#### SqlPersistenceConfigurationTests
- Tests SQL persistence setup
- Validates connection string requirements
- Tests custom schema configuration
- Tests table prefix handling
- Validates saga and subscription storage
- Tests outbox integration with SQL persistence

### Dependency Injection Tests

#### DependencyInjectionTests
- Tests service registration
- Validates isolated service providers
- Tests service resolution
- Covers scoped and singleton services

### Custom Configuration Tests

#### CustomConfigurationTests
- Tests custom recoverability settings
- Validates custom endpoint naming
- Tests database connection creation
- Tests serialization customization
- Covers async configuration hooks

### Integration Tests

#### EndpointIntegrationTests
- Tests complete end-to-end scenarios
- Validates multiple concurrent endpoints
- Tests long-running operations
- Tests restart scenarios
- Stress tests for memory leaks

### Error Handling Tests

#### ErrorHandlingTests
- Tests invalid configuration handling
- Validates exception scenarios
- Tests edge cases (null, empty, negative values)
- Tests disposal safety
- Covers timeout and cancellation scenarios

## Test Helpers

### TestEndpoints.cs
Provides test endpoint implementations:
- **TestEndpoint**: Basic test endpoint with tracking flags
- **FaultyTransportEndpoint**: Endpoint that throws during configuration
- **CustomNamedEndpoint**: Endpoint with custom naming
- **SqlPersistenceEndpoint**: Endpoint with SQL persistence
- **CustomRecoverabilityEndpoint**: Endpoint with custom retry settings
- **DependencyInjectionEndpoint**: Endpoint with DI setup
- **MockDbConnection**: Mock database connection for testing

## Running Tests

### Run All Tests
```bash
dotnet test ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj
```

### Run Specific Test Class
```bash
dotnet test --filter FullyQualifiedName~ClearHostedEndpoint.Tests.ClearHostedEndpointTests
```

### Run with Detailed Output
```bash
dotnet test ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj --logger "console;verbosity=detailed"
```

### Run with Coverage
```bash
dotnet test ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj /p:CollectCoverage=true
```

## Test Categories

Tests are organized by functionality:
1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test end-to-end scenarios
3. **Configuration Tests**: Test configuration options
4. **Lifecycle Tests**: Test endpoint lifecycle management
5. **Error Handling Tests**: Test error conditions and edge cases

## Test Patterns

### Arrange-Act-Assert
All tests follow the AAA pattern:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var endpoint = new TestEndpoint();
    
    try
    {
        // Act
        await endpoint.StartAsync(CancellationToken.None);
        
        // Assert
        endpoint.SomeProperty.Should().BeTrue();
    }
    finally
    {
        await endpoint.StopAsync(CancellationToken.None);
        endpoint.Dispose();
    }
}
```

### Theory-Based Testing
Parameterized tests for multiple scenarios:
```csharp
[Theory]
[InlineData(1)]
[InlineData(5)]
[InlineData(10)]
public async Task Test_WithDifferentValues(int value)
{
    // Test implementation
}
```

### Resource Cleanup
All tests properly dispose resources:
```csharp
try
{
    await endpoint.StartAsync(CancellationToken.None);
    // Test logic
}
finally
{
    await endpoint.StopAsync(CancellationToken.None);
    endpoint.Dispose();
}
```

## Dependencies

- **xUnit**: Testing framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework
- **NServiceBus**: For endpoint functionality
- **NServiceBus.Testing**: NServiceBus testing utilities

## Test Statistics

- **Total Test Classes**: 11
- **Total Test Methods**: 150+
- **Configuration Tests**: 40+
- **Lifecycle Tests**: 30+
- **Integration Tests**: 20+
- **Error Handling Tests**: 25+
- **Custom Configuration Tests**: 15+

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Fast execution (most tests complete in < 1 second)
- No external dependencies required
- Deterministic results
- Proper resource cleanup

## Contributing

When adding new features to ClearHostedEndpoint:
1. Add corresponding unit tests
2. Follow existing test patterns
3. Ensure proper cleanup in tests
4. Update this README if adding new test categories
5. Run all tests before submitting PR

## Notes

- Tests use LearningTransport to avoid external transport dependencies
- SQL persistence tests use mock connections to avoid database dependencies
- Tests are isolated and can run in parallel
- All async operations properly handle cancellation tokens
- Tests verify both success and failure scenarios
