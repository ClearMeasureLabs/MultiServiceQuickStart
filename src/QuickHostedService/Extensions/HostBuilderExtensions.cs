using Microsoft.Extensions.Hosting;
using Serilog;

namespace ClearMeasure.HostedService.Extensions;

/// <summary>
/// Extension methods for <see cref="IHostBuilder"/> to simplify host configuration.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures Serilog as the logging provider for the host.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="configureLogger">Optional action to configure the logger.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UseQuickHostedServiceLogging(
        this IHostBuilder builder,
        Action<LoggerConfiguration>? configureLogger = null)
    {
        return builder.UseSerilog((context, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            configureLogger?.Invoke(configuration);
        });
    }
}
