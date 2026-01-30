# ClearHostedService - Project Documentation

## Documentation Index

This project includes comprehensive documentation to help you understand, and use ClearHostedService.

### For Users

#### ?? [README.md](README.md)
Start here! Overview of the project, quick start guide, and key features.

#### ?? [USAGE.md](USAGE.md)
Detailed usage guide with examples for common scenarios:
- Basic usage
- Dependency injection
- Logging configuration
- Lifecycle hooks
- Error handling
- Advanced patterns

#### ?? [API.md](API.md)
Complete API reference documentation:
- `ClearHostedService` class
- All methods and properties
- Configuration options
- Extension methods
- Best practices

#### ?? [examples/README.md](examples/README.md)
Sample implementations showing real-world usage:
- Simple worker
- Data processor
- Message queue consumer
- Scheduled tasks
- Resilient services
- Multi-tenant services

### For Contributors and Maintainers

#### ??? [ARCHITECTURE.md](ARCHITECTURE.md)
Architectural overview and design:
- Onion Architecture structure
- Component responsibilities
- Dependency injection strategy
- Logging architecture
- Lifecycle management
- Extensibility points

#### ?? [DESIGN_DECISIONS.md](DESIGN_DECISIONS.md)
Rationale behind key design decisions:
- Why isolated service collections?
- Why Serilog?
- Why abstract vs virtual methods?
- Trade-offs and alternatives considered

#### ?? [CHANGELOG.md](CHANGELOG.md)
Version history and changes:
- New features
- Bug fixes
- Breaking changes
- Migration guides

## Documentation Structure

```
ClearHostedService/
??? README.md                      # Project overview and quick start
??? ARCHITECTURE.md                # Architecture and design
??? USAGE.md                       # Detailed usage guide
??? API.md                         # API reference
??? CONTRIBUTING.md                # Contribution guidelines
??? CHANGELOG.md                   # Version history
??? DESIGN_DECISIONS.md           # Design rationale
??? examples/
    ??? README.md                  # Examples overview
```

## Quick Links by Topic

### Getting Started
- [Installation](README.md#quick-start)
- [First Service](USAGE.md#basic-usage)
- [Examples](examples/README.md)

### Architecture & Design
- [Onion Architecture](ARCHITECTURE.md#onion-architecture)
- [DI Strategy](ARCHITECTURE.md#dependency-injection-strategy)
- [Lifecycle](ARCHITECTURE.md#lifecycle-management)
- [Design Rationale](DESIGN_DECISIONS.md)

### Features
- [Logging](USAGE.md#logging-configuration)
- [Dependency Injection](USAGE.md#dependency-injection)
- [Lifecycle Hooks](USAGE.md#lifecycle-hooks)
- [Error Handling](USAGE.md#error-handling)

### API Reference
- [ClearHostedService](API.md#ClearHostedService)
- [Methods](API.md#methods)
- [Properties](API.md#properties)
- [Configuration](API.md#configuration)

### Contributing
- [Development Setup](CONTRIBUTING.md#building-and-testing)
- [Coding Standards](CONTRIBUTING.md#coding-standards)
- [Testing](CONTRIBUTING.md#testing-requirements)
- [Pull Requests](CONTRIBUTING.md#pull-requests)

## Documentation Principles

Our documentation follows these principles:

1. **Comprehensive**: Cover all aspects of the library
2. **Accessible**: Clear language, good examples
3. **Up-to-Date**: Synchronized with code changes
4. **Searchable**: Good headings, index, and structure
5. **Progressive**: From simple to advanced

## Keeping Documentation Updated

When making changes to the code:

- [ ] Update API.md if public API changes
- [ ] Update USAGE.md if usage patterns change
- [ ] Update ARCHITECTURE.md if structure changes
- [ ] Update README.md if features change
- [ ] Add entry to CHANGELOG.md
- [ ] Update examples if relevant

## Feedback

Documentation improvements are always welcome!

- Found an error? [Open an issue](../../issues)
- Have a suggestion? [Start a discussion](../../discussions)

## License

All documentation is licensed under the same license as the project.
