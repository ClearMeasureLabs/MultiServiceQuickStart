using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Test endpoint for testing ClearHostedEndpoint functionality.
/// </summary>
public class TestEndpoint : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
{
    public bool ConfigureTransportCalled { get; private set; }
    public bool ConfigureEndpointCalled { get; private set; }
    public bool ConfigureSerializationCalled { get; private set; }
    public bool ConfigurePersistenceCalled { get; private set; }
    public bool ConfigureRecoverabilityCalled { get; private set; }
    public bool ConfigureEndpointAsyncCalled { get; private set; }
    public bool ExecuteAsyncCalled { get; private set; }

    private readonly EndpointOptions? _customEndpointOptions;
    private readonly SqlPersistenceOptions? _customSqlPersistenceOptions;
    private readonly Action<EndpointConfiguration>? _transportConfigurator;

    public TestEndpoint(
        IConfiguration? configuration = null,
        EndpointOptions? endpointOptions = null,
        SqlPersistenceOptions? sqlPersistenceOptions = null,
        Action<EndpointConfiguration>? transportConfigurator = null)
        : base(configuration ?? new ConfigurationBuilder().Build())
    {
        _customEndpointOptions = endpointOptions;
        _customSqlPersistenceOptions = sqlPersistenceOptions;
        _transportConfigurator = transportConfigurator;
    }

    protected override EndpointOptions EndpointOptions => _customEndpointOptions ?? base.EndpointOptions;

    protected override SqlPersistenceOptions? SqlPersistenceOptions => _customSqlPersistenceOptions;

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        ConfigureTransportCalled = true;
        
        // Use the custom configurator if provided, otherwise use LearningTransport
        if (_transportConfigurator != null)
        {
            _transportConfigurator(endpointConfiguration);
        }
        else
        {
            endpointConfiguration.UseTransport<LearningTransport>();
        }
    }

    protected override void ConfigureEndpoint(EndpointConfiguration endpointConfiguration)
    {
        ConfigureEndpointCalled = true;
        base.ConfigureEndpoint(endpointConfiguration);
    }

    protected override void ConfigureSerialization(EndpointConfiguration endpointConfiguration)
    {
        ConfigureSerializationCalled = true;
        base.ConfigureSerialization(endpointConfiguration);
    }

    protected override void ConfigurePersistence(EndpointConfiguration endpointConfiguration)
    {
        ConfigurePersistenceCalled = true;
        base.ConfigurePersistence(endpointConfiguration);
    }

    protected override void ConfigureRecoverability(EndpointConfiguration endpointConfiguration)
    {
        ConfigureRecoverabilityCalled = true;
        base.ConfigureRecoverability(endpointConfiguration);
    }

    protected override Task ConfigureEndpointAsync(EndpointConfiguration endpointConfiguration, CancellationToken cancellationToken)
    {
        ConfigureEndpointAsyncCalled = true;
        return base.ConfigureEndpointAsync(endpointConfiguration, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ExecuteAsyncCalled = true;
        await base.ExecuteAsync(stoppingToken);
    }
}

/// <summary>
/// Test endpoint that throws an exception during transport configuration.
/// </summary>
public class FaultyTransportEndpoint : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
{
    public FaultyTransportEndpoint(IConfiguration configuration) : base(configuration)
    {
    }
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        throw new InvalidOperationException("Transport configuration failed");
    }
}

/// <summary>
/// Test endpoint that uses custom endpoint name.
/// </summary>
public class CustomNamedEndpoint : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
{
    private readonly string _endpointName;

    public CustomNamedEndpoint(IConfiguration configuration, string endpointName) : base(configuration)
    {
        _endpointName = endpointName;
    }

    protected override EndpointOptions EndpointOptions => new() { EndpointName = _endpointName };

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseTransport<LearningTransport>();
    }
}

/// <summary>
/// Test endpoint with SQL persistence configured.
/// </summary>
public class SqlPersistenceEndpoint : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
{
    private readonly SqlPersistenceOptions _sqlOptions;

    public SqlPersistenceEndpoint(IConfiguration configuration, SqlPersistenceOptions sqlOptions) : base(configuration)
    {
        _sqlOptions = sqlOptions;
    }

    protected override SqlPersistenceOptions? SqlPersistenceOptions => _sqlOptions;

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseTransport<LearningTransport>();
    }

    protected override DbConnection CreateDbConnection(string connectionString)
    {
        // Return a mock connection for testing
        return new MockDbConnection(connectionString);
    }
}

/// <summary>
/// Mock database connection for testing.
/// </summary>
public class MockDbConnection : DbConnection
{
    private readonly string _connectionString;

    public MockDbConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override string ConnectionString
    {
        get => _connectionString;
        set { }
    }

    public override string Database => "TestDatabase";

    public override string DataSource => "TestDataSource";

    public override string ServerVersion => "1.0.0";

    public override ConnectionState State => ConnectionState.Closed;

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close()
    {
    }

    public override void Open()
    {
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }

    protected override DbCommand CreateDbCommand()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Test endpoint with custom recoverability configuration.
/// </summary>
public class CustomRecoverabilityEndpoint : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
{
    public CustomRecoverabilityEndpoint(IConfiguration configuration) : base(configuration)
    {
    }

    public int CustomImmediateRetries { get; set; } = 5;
    public int CustomDelayedRetries { get; set; } = 10;

    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseTransport<LearningTransport>();
    }

    protected override void ConfigureRecoverability(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.Recoverability()
            .Immediate(immediate => immediate.NumberOfRetries(CustomImmediateRetries))
            .Delayed(delayed => delayed.NumberOfRetries(CustomDelayedRetries));
    }
}

/// <summary>
/// Test endpoint that registers custom dependencies.
/// </summary>
public class DependencyInjectionEndpoint : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
{
    public DependencyInjectionEndpoint(IConfiguration configuration) : base(configuration)
    {
    }
    protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseTransport<LearningTransport>();
    }

    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        services.AddSingleton<ITestService, TestService>();
        services.AddScoped<IScopedTestService, ScopedTestService>();
    }
}

public interface ITestService
{
    void DoWork();
}

public class TestService : ITestService
{
    public bool WorkCalled { get; private set; }

    public void DoWork()
    {
        WorkCalled = true;
    }
}

public interface IScopedTestService
{
    void DoScopedWork();
}

public class ScopedTestService : IScopedTestService
{
    public bool ScopedWorkCalled { get; private set; }

    public void DoScopedWork()
    {
        ScopedWorkCalled = true;
    }
}
