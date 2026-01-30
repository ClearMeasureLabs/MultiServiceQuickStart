using NServiceBus;
using NServiceBus.Pipeline;
using NServiceBus.Persistence.Sql;
using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ClearMeasure.HostedEndpoint.SqlServerTransport;

/// <summary>
/// NServiceBus behavior that attaches the synchronized storage session's database connection
/// and transaction to the StorageContext for use in message handlers.
/// </summary>
/// <remarks>
/// This behavior ensures that all database operations performed through StorageContext
/// participate in the same transaction as the NServiceBus message processing,
/// preventing escalation to distributed transactions.
/// </remarks>
public class StorageContextBehavior : Behavior<IInvokeHandlerContext>
{
    /// <summary>
    /// Invokes the behavior to attach the storage context.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        // Get the synchronized storage session from the message context
        var session = context.SynchronizedStorageSession.SqlPersistenceSession();

        // Resolve the StorageContext from the message handler's DI container
        var storageContext = context.Extensions.Get<IServiceProvider>().GetRequiredService<StorageContext>();

        // Attach the connection and transaction to the storage context
        storageContext.Attach(session.Connection, session.Transaction);

        // Continue with the next behavior in the pipeline
        await next();
    }
}
