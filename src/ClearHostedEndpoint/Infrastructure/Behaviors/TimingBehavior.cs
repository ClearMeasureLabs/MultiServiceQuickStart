using System.Diagnostics;
using NServiceBus.Pipeline;
using Serilog;

namespace ClearMeasure.HostedEndpoint.Infrastructure.Behaviors;

/// <summary>
/// NServiceBus pipeline behavior that logs handler execution time with Application Insights metrics.
/// </summary>
public class TimingBehavior : Behavior<IInvokeHandlerContext>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimingBehavior"/> class.
    /// </summary>
    /// <param name="logger">The Serilog logger instance.</param>
    public TimingBehavior(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the behavior and logs the handler execution time.
    /// </summary>
    /// <param name="context">The handler context.</param>
    /// <param name="next">The next step in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageType = context.MessageBeingHandled.GetType().Name;
        var handlerType = context.MessageHandler.HandlerType.Name;

        try
        {
            await next();
            stopwatch.Stop();

            _logger.Information(
                "Handler {HandlerType} processed message {MessageType} in {ElapsedMilliseconds}ms",
                handlerType,
                messageType,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.Error(
                ex,
                "Handler {HandlerType} failed to process message {MessageType} after {ElapsedMilliseconds}ms",
                handlerType,
                messageType,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
