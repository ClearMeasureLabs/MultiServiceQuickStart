# Design Decisions

This document explains key design decisions made in ClearHostedService and the rationale behind them.

## 1. Isolated Service Collections

### Decision
Each `ClearHostedService` instance maintains its own `IServiceCollection` and `IServiceProvider`.

### Rationale
- **Isolation**: Services don't interfere with each other
- **Independence**: Each service can be configured, started, stopped independently
- **Clarity**: Dependencies are explicit and scoped to the service
- **Testing**: Easier to test services in isolation

### Trade-offs
- **Memory**: Slightly higher memory usage per service
- **Shared State**: Cannot easily share scoped instances between services
- **Complexity**: Each service manages its own DI container

### Alternative Considered
Using a single shared `IServiceProvider` was considered but rejected because:
- It would couple services together
- Lifecycle management would be more complex
- Testing would require more setup
- Services couldn't have conflicting dependencies

---

## 2. Serilog as Default Logger

### Decision
Use Serilog instead of the default .NET logging framework.

### Rationale
- **Structured Logging**: First-class support for structured data
- **Rich Ecosystem**: Many sinks and enrichers available
- **Performance**: Excellent performance characteristics
- **ApplicationInsights**: Seamless integration with Azure monitoring
- **Flexibility**: Easy to configure multiple output targets

### Trade-offs
- **Dependency**: Adds external dependency
- **Learning Curve**: Developers need to learn Serilog conventions
- **Configuration**: More configuration options can be overwhelming

### Alternative Considered
Microsoft.Extensions.Logging was considered but:
- Less powerful structured logging
- More boilerplate for ApplicationInsights
- Fewer enrichment options

---

## 3. Abstract ExecuteAsync Method

### Decision
Make `ExecuteAsync` an abstract method that child classes must implement.

### Rationale
- **Enforces Implementation**: Prevents accidental empty services
- **Clear Contract**: Makes it obvious where business logic goes
- **Compile-Time Safety**: Ensures the method is implemented
- **Consistency**: All services have the same structure

### Trade-offs
- **Flexibility**: Can't have optional execution
- **Boilerplate**: Must always implement even if minimal

### Alternative Considered
Making it virtual with a default implementation:
- Would allow services to skip implementation
- Could lead to incomplete services
- Less explicit about requirements

---

## 4. Virtual Lifecycle Hooks

### Decision
Make `OnStartingAsync` and `OnStoppingAsync` virtual (optional overrides).

### Rationale
- **Optional**: Not all services need lifecycle hooks
- **Flexibility**: Services can add startup/shutdown logic when needed
- **Sensible Defaults**: Base implementation does nothing
- **Non-Breaking**: Can add hooks later without changing interface

### Trade-offs
- **Discoverability**: Developers might not know these exist
- **Consistency**: Some services use them, others don't

### Alternative Considered
Making them abstract (required):
- Would force boilerplate in simple services
- Not all services need these hooks
- Would reduce usability

---

## 5. Onion Architecture

### Decision
Structure the library following Onion Architecture principles.

### Rationale
- **Maintainability**: Clear separation of concerns
- **Testability**: Easy to mock dependencies
- **Flexibility**: Infrastructure can be swapped
- **Clarity**: Dependencies flow in one direction (inward)
- **Industry Standard**: Well-understood pattern

### Trade-offs
- **Complexity**: More layers than simple layering
- **Overhead**: More files and abstractions
- **Learning Curve**: Developers need to understand the pattern

### Alternative Considered
Simple three-tier architecture:
- Less flexible
- Harder to test
- More coupling between layers

---

## 6. RegisterDependencyInjection Method

### Decision
Provide a virtual `RegisterDependencyInjection` method instead of constructor injection.

### Rationale
- **Simplicity**: Single place to register all dependencies
- **Discoverability**: Clear where DI setup happens
- **Flexibility**: Can add complex registration logic
- **Isolation**: Registration happens within the service's context

### Trade-offs
- **Timing**: Called during service construction, not at app startup
- **Convention**: Less common than constructor injection

### Alternative Considered
Constructor injection pattern:
- Would require passing `IServiceCollection` through constructor
- Less clear when/how services are registered
- Would complicate the base class constructor

---

## 7. Graceful Shutdown with Timeout

### Decision
Implement graceful shutdown with a configurable timeout.

### Rationale
- **Reliability**: Ensures services don't hang during shutdown
- **Data Integrity**: Allows in-progress work to complete
- **Flexibility**: Timeout can be configured per deployment
- **Best Practice**: Standard pattern for hosted services

### Trade-offs
- **Complexity**: More code to handle timeouts
- **Configuration**: Another setting to manage

### Alternative Considered
Hard shutdown:
- Could lose in-progress work
- Could corrupt data
- Less production-ready

---

## 8. No Built-in Configuration File Support

### Decision
Don't include built-in configuration file support in the base class.

### Rationale
- **Flexibility**: Services can use any configuration source
- **Simplicity**: Keeps base class focused
- **Composition**: Configuration can be injected via DI
- **No Opinions**: Don't force a configuration pattern

### Trade-offs
- **Convenience**: More setup required
- **Documentation**: Need to show how to add configuration

### Alternative Considered
Built-in appsettings.json support:
- Would add complexity
- Would limit flexibility
- Would introduce file I/O in the base class

---

## 9. Target .NET 10.0

### Decision
Target .NET 10.0 as the minimum version.

### Rationale
- **Modern Features**: Access to latest C# and .NET features
- **Performance**: Benefit from latest runtime optimizations
- **Support**: Long-term support version
- **Dependencies**: Latest dependency injection features

### Trade-offs
- **Compatibility**: Can't be used on older .NET versions
- **Adoption**: Some organizations may not have upgraded

### Alternative Considered
Multi-targeting:
- Would increase complexity
- Would limit features we can use
- Would increase maintenance burden

---

## Summary

These design decisions prioritize:

1. **Isolation and Independence**: Each service is self-contained
2. **Flexibility**: Services can be customized without constraints
3. **Production Readiness**: Proper logging, shutdown, error handling
4. **Maintainability**: Clean architecture and clear patterns
5. **Simplicity**: Minimal required implementation

The library is opinionated where it helps (logging, DI, architecture) but flexible where it matters (service implementation, configuration, deployment).
