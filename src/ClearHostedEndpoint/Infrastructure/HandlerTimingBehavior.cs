using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using NServiceBus;
using NServiceBus.Pipeline;

namespace ClearMeasure.HostedEndpoint.Infrastructure;

/// <summary>
/// NServiceBus pipeline behavior that tracks handler execution time and sends metrics to Application Insights.
/// </summary>
public class HandlerTimingBehavior : Behavior<IInvokeHandlerContext>
{
    private readonly TelemetryClient? _telemetryClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerTimingBehavior"/> class.
    /// </summary>
    /// <param name="telemetryClient">Optional telemetry client for Application Insights tracking.</param>
    public HandlerTimingBehavior(TelemetryClient? telemetryClient = null)
    {
        _telemetryClient = telemetryClient;
    }

    /// <summary>
    /// Invokes the behavior to track handler execution time.
    /// </summary>
    /// <param name="context">The handler context.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var startTime = DateTimeOffset.UtcNow;
        var handlerType = context.MessageHandler.HandlerType;
        var messageType = context.MessageBeingHandled.GetType();
        
        Exception? exception = null;
        
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            var duration = DateTimeOffset.UtcNow - startTime;
            
            if (_telemetryClient != null)
            {
                TrackHandlerExecution(handlerType, messageType, duration, exception, context);
            }
        }
    }

    private void TrackHandlerExecution(
        Type handlerType,
        Type messageType,
        TimeSpan duration,
        Exception? exception,
        IInvokeHandlerContext context)
    {
        // Track as a dependency telemetry (represents a call to handle a message)
        var dependencyTelemetry = new DependencyTelemetry
        {
            Name = $"{handlerType.Name}.Handle({messageType.Name})",
            Type = "NServiceBus.Handler",
            Duration = duration,
            Success = exception == null,
            Timestamp = DateTimeOffset.UtcNow - duration
        };

        // Add custom properties for better filtering and analysis
        dependencyTelemetry.Properties["HandlerType"] = handlerType.FullName ?? handlerType.Name;
        dependencyTelemetry.Properties["MessageType"] = messageType.FullName ?? messageType.Name;
        dependencyTelemetry.Properties["MessageId"] = context.MessageId;
        
        if (context.Headers.TryGetValue(NServiceBus.Headers.CorrelationId, out var correlationId))
        {
            dependencyTelemetry.Properties["CorrelationId"] = correlationId;
        }

        if (context.Headers.TryGetValue(NServiceBus.Headers.ConversationId, out var conversationId))
        {
            dependencyTelemetry.Properties["ConversationId"] = conversationId;
        }

        if (exception != null)
        {
            dependencyTelemetry.Properties["ExceptionType"] = exception.GetType().FullName ?? exception.GetType().Name;
            dependencyTelemetry.Properties["ExceptionMessage"] = exception.Message;
        }

        _telemetryClient?.TrackDependency(dependencyTelemetry);
    }
}
