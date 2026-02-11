using NServiceBus.Pipeline;
using Serilog;
using System.Diagnostics;

namespace ClearMeasure.HostedEndpoint.Behaviors;

/// <summary>
/// NServiceBus pipeline behavior that captures and logs handler execution timing metrics.
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
    /// Invoked when a message is being handled.
    /// </summary>
    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageType = context.MessageBeingHandled.GetType().Name;
        var handlerType = context.MessageHandler.Instance.GetType().Name;

        try
        {
            _logger.Information(
                "Handler {HandlerType} starting to process message {MessageType}",
                handlerType,
                messageType);

            await next();

            stopwatch.Stop();

            _logger.Information(
                "Handler {HandlerType} completed processing message {MessageType} in {ElapsedMilliseconds}ms",
                handlerType,
                messageType,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.Error(
                ex,
                "Handler {HandlerType} failed processing message {MessageType} after {ElapsedMilliseconds}ms",
                handlerType,
                messageType,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
