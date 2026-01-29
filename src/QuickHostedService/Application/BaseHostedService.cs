using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickHostedService.Core.Exceptions;
using QuickHostedService.Core.Interfaces;
using QuickHostedService.Infrastructure.Configuration;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace QuickHostedService.Application;

/// <summary>
/// Base class for hosted services that provides dependency injection, logging, and lifecycle management.
/// </summary>
public abstract class BaseHostedService : IHostedService, IHostedServiceLifecycle, IDisposable
{
    private IServiceProvider? _serviceProvider;
    private ILogger? _logger;
    private Task? _executingTask;
    private CancellationTokenSource? _stoppingCts;
    private bool _disposed;

    /// <summary>
    /// Gets the service provider for this hosted service instance.
    /// Use this to resolve dependencies registered in <see cref="RegisterDependencyInjection"/>.
    /// </summary>
    protected IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException(
                    "ServiceProvider is not available. This should only be accessed after StartAsync has been called.");
            }
            return _serviceProvider;
        }
    }

    /// <summary>
    /// Gets the logger for this hosted service instance.
    /// </summary>
    protected ILogger Logger
    {
        get
        {
            if (_logger == null)
            {
                throw new InvalidOperationException(
                    "Logger is not available. This should only be accessed after StartAsync has been called.");
            }
            return _logger;
        }
    }

    /// <summary>
    /// Gets the hosted service options.
    /// </summary>
    protected virtual HostedServiceOptions Options { get; } = new();

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Configure logging first
            ConfigureLogging();

            _logger = Log.Logger;
            _logger.Information("Starting hosted service: {ServiceType}", GetType().Name);

            // Build service collection
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: false);
            });

            // Register dependencies
            try
            {
                RegisterDependencyInjection(services);
            }
            catch (Exception ex)
            {
                throw new ServiceRegistrationException(
                    "Failed to register dependencies. See inner exception for details.", ex);
            }

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();

            // Call startup hook
            await OnStartingAsync(cancellationToken);

            // Create cancellation token source
            _stoppingCts = new CancellationTokenSource();

            // Start executing
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            // If the task is completed, await it to propagate any exceptions
            if (_executingTask.IsCompleted)
            {
                await _executingTask;
            }

            _logger.Information("Hosted service started successfully: {ServiceType}", GetType().Name);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Failed to start hosted service: {ServiceType}", GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger?.Information("Stopping hosted service: {ServiceType}", GetType().Name);

            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts?.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                var delayTask = Task.Delay(Options.ShutdownTimeout, cancellationToken);
                var completedTask = await Task.WhenAny(_executingTask, delayTask);

                if (completedTask == delayTask)
                {
                    _logger?.Warning(
                        "Hosted service did not stop within the timeout period ({Timeout}): {ServiceType}",
                        Options.ShutdownTimeout,
                        GetType().Name);
                }
            }

            // Call stopping hook
            await OnStoppingAsync(cancellationToken);

            _logger?.Information("Hosted service stopped: {ServiceType}", GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error stopping hosted service: {ServiceType}", GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Configures Serilog logging for the hosted service.
    /// Override this method to customize logging configuration.
    /// </summary>
    protected virtual void ConfigureLogging()
    {
        var options = GetLoggingOptions();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(options.LogLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();

        if (options.EnableConsoleLogging)
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: options.OutputTemplate);
        }

        if (options.EnableFileLogging)
        {
            var logFilePath = Path.Combine(
                options.LogDirectory,
                $"{GetType().Name}-.txt");

            loggerConfig.WriteTo.File(
                logFilePath,
                rollingInterval: options.RollingInterval,
                outputTemplate: options.OutputTemplate);
        }

        Log.Logger = loggerConfig.CreateLogger();
    }

    /// <summary>
    /// Gets the logging options for this hosted service.
    /// Override this method to provide custom logging options.
    /// </summary>
    /// <returns>The logging options.</returns>
    protected virtual LoggingOptions GetLoggingOptions()
    {
        return new LoggingOptions();
    }

    /// <summary>
    /// Override this method to register application-specific dependencies into the service collection.
    /// </summary>
    /// <param name="services">The service collection to register dependencies into.</param>
    protected virtual void RegisterDependencyInjection(IServiceCollection services)
    {
        // Default implementation - no additional services
    }

    /// <summary>
    /// Called when the hosted service is starting.
    /// Override this method to perform initialization logic before the main execution begins.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe during startup.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnStartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the hosted service is stopping.
    /// Override this method to perform cleanup logic before resources are disposed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe during shutdown.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method is called when the <see cref="IHostedService"/> starts.
    /// Implement this method to define the main execution logic for your hosted service.
    /// </summary>
    /// <param name="stoppingToken">Triggered when the hosted service should stop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

    /// <summary>
    /// Disposes of resources used by the hosted service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of resources used by the hosted service.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _stoppingCts?.Cancel();
            _stoppingCts?.Dispose();

            if (_serviceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }
        }

        _disposed = true;
    }
}
