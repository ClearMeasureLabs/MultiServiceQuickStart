using Microsoft.Extensions.Logging;
using NServiceBus.Pipeline;
using System.Diagnostics;

namespace ClearMeasure.HostedEndpoint.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that logs the execution time of NServiceBus message handlers.
/// </summary>
public class TimingBehavior : Behavior<IInvokeHandlerContext>
{
    private readonly ILogger<TimingBehavior> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimingBehavior"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public TimingBehavior(ILogger<TimingBehavior> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the behavior and logs the execution time.
    /// </summary>
    /// <param name="context">The handler invocation context.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageType = context.MessageBeingHandled?.GetType().Name ?? "Unknown";
        var handlerType = context.MessageHandler.HandlerType.Name;

        try
        {
            await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Handler {HandlerType} processed message {MessageType} in {ElapsedMilliseconds}ms",
                handlerType,
                messageType,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Handler {HandlerType} failed processing message {MessageType} after {ElapsedMilliseconds}ms",
                handlerType,
                messageType,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
