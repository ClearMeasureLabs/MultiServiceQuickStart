using NServiceBus;
using NServiceBus.Pipeline;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ClearMeasure.HostedEndpoint.Behaviors;

/// <summary>
/// NServiceBus behavior that tracks message handler execution time and logs metrics.
/// This enables performance monitoring in Application Insights.
/// </summary>
public class TimingBehavior : Behavior<IInvokeHandlerContext>
{
    private readonly ILogger<TimingBehavior> _logger;

    public TimingBehavior(ILogger<TimingBehavior> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var messageType = context.MessageBeingHandled.GetType().Name;
        var handlerType = context.MessageHandler.Instance.GetType().Name;
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await next();
            
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Message handler completed. MessageType={MessageType}, HandlerType={HandlerType}, Duration={Duration}ms, MessageId={MessageId}",
                messageType,
                handlerType,
                stopwatch.ElapsedMilliseconds,
                context.MessageId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(
                ex,
                "Message handler failed. MessageType={MessageType}, HandlerType={HandlerType}, Duration={Duration}ms, MessageId={MessageId}",
                messageType,
                handlerType,
                stopwatch.ElapsedMilliseconds,
                context.MessageId);
            
            throw;
        }
    }
}
