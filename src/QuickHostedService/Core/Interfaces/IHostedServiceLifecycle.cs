namespace QuickHostedService.Core.Interfaces;

/// <summary>
/// Defines lifecycle hooks for hosted services.
/// </summary>
public interface IHostedServiceLifecycle
{
    /// <summary>
    /// Called when the hosted service is starting.
    /// Perform initialization logic here before the main execution begins.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe during startup.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnStartingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Called when the hosted service is stopping.
    /// Perform cleanup logic here before resources are disposed.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe during shutdown.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnStoppingAsync(CancellationToken cancellationToken);
}
