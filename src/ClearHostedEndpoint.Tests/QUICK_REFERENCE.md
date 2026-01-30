# ClearHostedEndpoint Tests - Quick Reference

## Test Execution Commands

```bash
# Run all tests
dotnet test ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj

# Run with minimal output
dotnet test ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj --logger "console;verbosity=minimal"

# Run specific test class
dotnet test --filter "FullyQualifiedName~EndpointOptionsTests"

# Run tests by category
dotnet test --filter "FullyQualifiedName~Configuration"
dotnet test --filter "FullyQualifiedName~Lifecycle"
dotnet test --filter "FullyQualifiedName~Integration"
```

## Test Organization

### ?? Configuration/
- `EndpointOptionsTests.cs` - Endpoint configuration validation (20 tests)
- `SqlPersistenceOptionsTests.cs` - SQL persistence configuration (14 tests)

### ?? Exceptions/
- `EndpointConfigurationExceptionTests.cs` - Exception handling (11 tests)

### ?? Root Test Files
- `ClearHostedEndpointTests.cs` - Core endpoint functionality (15 tests)
- `EndpointLifecycleTests.cs` - Lifecycle management (15 tests)
- `EndpointOptionsConfigurationTests.cs` - Option integration (10 tests)
- `SqlPersistenceConfigurationTests.cs` - SQL persistence integration (12 tests)
- `DependencyInjectionTests.cs` - DI functionality (4 tests)
- `CustomConfigurationTests.cs` - Custom configurations (8 tests)
- `EndpointIntegrationTests.cs` - End-to-end tests (12 tests)
- `ErrorHandlingTests.cs` - Error scenarios (15 tests)

### ?? Test Helpers
- `TestEndpoints.cs` - Test endpoint implementations and mocks

## Test Statistics

| Metric | Value |
|--------|-------|
| **Total Tests** | 153 |
| **Passing** | 153 (100%) |
| **Failing** | 0 |
| **Duration** | ~10 seconds |
| **Test Classes** | 11 |
| **Helper Classes** | 7 |

## Coverage Areas

### ? Fully Covered
- All EndpointOptions properties
- All SqlPersistenceOptions properties
- Endpoint lifecycle (start/stop/dispose)
- Error handling and exceptions
- Dependency injection
- Custom configurations
- Concurrent endpoints
- Resource cleanup

### ?? Partial Coverage
- Outbox configuration (limited by LearningTransport)
- Real database persistence (uses mocks)

## Test Patterns Used

1. **Arrange-Act-Assert (AAA)**
2. **Theory-based testing** with InlineData
3. **Resource cleanup** with try-finally
4. **Isolated tests** (no shared state)
5. **Mock objects** for external dependencies

## Common Test Scenarios

### Testing Endpoint Configuration
```csharp
[Fact]
public async Task Endpoint_WithCustomOptions_ShouldStartSuccessfully()
{
    var options = new EndpointOptions { MaxConcurrency = 5 };
    var endpoint = new TestEndpoint(endpointOptions: options);
    try
    {
        await endpoint.StartAsync(CancellationToken.None);
        await Task.Delay(500);
        // Assert
    }
    finally
    {
        await endpoint.StopAsync(CancellationToken.None);
        endpoint.Dispose();
    }
}
```

### Testing Error Conditions
```csharp
[Fact]
public async Task Endpoint_WithInvalidConfig_ShouldThrowException()
{
    var endpoint = new FaultyEndpoint();
    var act = async () => await endpoint.StartAsync(CancellationToken.None);
    await act.Should().ThrowAsync<EndpointConfigurationException>();
    endpoint.Dispose();
}
```

### Theory-Based Testing
```csharp
[Theory]
[InlineData(1)]
[InlineData(5)]
[InlineData(10)]
public async Task Endpoint_WithVariousConcurrency_ShouldWork(int concurrency)
{
    var options = new EndpointOptions { MaxConcurrency = concurrency };
    // Test implementation
}
```

## Debugging Tests

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName=ClearHostedEndpoint.Tests.ClearHostedEndpointTests.StartAsync_WithLearningTransport_ShouldStartSuccessfully"
```

### View Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Debug in Visual Studio
1. Open test file
2. Right-click on test method
3. Select "Debug Test"

## CI/CD Integration

### GitHub Actions
```yaml
- name: Run Tests
  run: dotnet test ClearHostedEndpoint.Tests/ClearHostedEndpoint.Tests.csproj --logger "trx;LogFileName=test-results.trx"
```

### Azure Pipelines
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: 'ClearHostedEndpoint.Tests/ClearHostedEndpoint.Tests.csproj'
    arguments: '--logger trx'
```

## Test Maintenance

### Adding New Tests
1. Choose appropriate test file or create new one
2. Follow AAA pattern
3. Use descriptive name: `Method_Scenario_ExpectedResult`
4. Add proper cleanup (try-finally)
5. Run locally before committing

### Updating Tests
1. Update test when API changes
2. Ensure backward compatibility where possible
3. Update documentation if patterns change

## Known Issues

### NServiceBus License Warnings
Tests show trial license warnings - this is expected and doesn't affect test results.

### Outbox Tests
Some outbox tests verify exceptions are thrown since LearningTransport doesn't support all features.

## Support

For issues or questions:
1. Check test output for details
2. Review TEST_SUMMARY.md for comprehensive information
3. Check README.md for project context
4. Review individual test files for examples

## Quick Test Checklist

Before committing:
- [ ] All tests pass locally
- [ ] New features have corresponding tests
- [ ] Tests follow existing patterns
- [ ] Resource cleanup is proper
- [ ] No hardcoded values (use configuration)
- [ ] Documentation updated if needed

---

**Last Updated**: 2026-01-30  
**Test Count**: 153  
**Pass Rate**: 100%  
**Avg Duration**: ~10 seconds
