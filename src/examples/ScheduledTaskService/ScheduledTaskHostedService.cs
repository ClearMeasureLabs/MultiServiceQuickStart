using QuickHostedService.Application;

namespace ScheduledTaskService;

/// <summary>
/// A hosted service that executes tasks on a schedule.
/// </summary>
public class ScheduledTaskHostedService : BaseHostedService
{
    private readonly TaskSchedule _schedule;

    public ScheduledTaskHostedService()
    {
        // Schedule daily execution at 2:00 AM
        // For demo purposes, we'll schedule it for every minute
        var now = DateTime.Now;
        _schedule = TaskSchedule.Daily(now.Hour, now.Minute + 1);
    }

    public override Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("ScheduledTaskHostedService initializing...");
        Logger.Information("Task scheduled for {Hour:D2}:{Minute:D2}", _schedule.Hour, _schedule.Minute);

        if (_schedule.DaysOfWeek != null && _schedule.DaysOfWeek.Length > 0)
        {
            Logger.Information("Running on days: {Days}", string.Join(", ", _schedule.DaysOfWeek));
        }
        else
        {
            Logger.Information("Running daily");
        }

        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.Information("ScheduledTaskHostedService started. Waiting for scheduled time...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = _schedule.GetNextRunTime();
            var now = DateTime.Now;
            var delay = nextRun - now;

            Logger.Information("Next execution scheduled for {ScheduledTime} (in {Delay})", 
                nextRun.ToString("yyyy-MM-dd HH:mm:ss"), 
                FormatTimeSpan(delay));

            try
            {
                // Wait until the scheduled time
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteScheduledTaskAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Information("Scheduled task was cancelled");
                break;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing scheduled task");
                // Continue to next scheduled run
            }
        }
    }

    private async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now;
        Logger.Information("========================================");
        Logger.Information("Executing scheduled task at {Time}", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Logger.Information("========================================");

        try
        {
            // Simulate task execution
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            // Example: Generate a report
            Logger.Information("Generating daily report...");
            await GenerateReportAsync(cancellationToken);

            // Example: Cleanup old data
            Logger.Information("Cleaning up old data...");
            await CleanupOldDataAsync(cancellationToken);

            // Example: Send notifications
            Logger.Information("Sending notifications...");
            await SendNotificationsAsync(cancellationToken);

            var duration = DateTime.Now - startTime;
            Logger.Information("========================================");
            Logger.Information("Scheduled task completed successfully in {Duration:F2} seconds", duration.TotalSeconds);
            Logger.Information("========================================");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Scheduled task failed");
            throw;
        }
    }

    private async Task GenerateReportAsync(CancellationToken cancellationToken)
    {
        // Simulate report generation
        await Task.Delay(500, cancellationToken);
        Logger.Information("Report generated: DailyReport_{Date}.pdf", DateTime.Now.ToString("yyyyMMdd"));
    }

    private async Task CleanupOldDataAsync(CancellationToken cancellationToken)
    {
        // Simulate data cleanup
        await Task.Delay(500, cancellationToken);
        var recordsDeleted = Random.Shared.Next(10, 100);
        Logger.Information("Cleaned up {Count} old records", recordsDeleted);
    }

    private async Task SendNotificationsAsync(CancellationToken cancellationToken)
    {
        // Simulate sending notifications
        await Task.Delay(500, cancellationToken);
        var notificationsSent = Random.Shared.Next(5, 20);
        Logger.Information("Sent {Count} notifications", notificationsSent);
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{timeSpan.TotalDays:F1} days";
        if (timeSpan.TotalHours >= 1)
            return $"{timeSpan.TotalHours:F1} hours";
        if (timeSpan.TotalMinutes >= 1)
            return $"{timeSpan.TotalMinutes:F1} minutes";
        return $"{timeSpan.TotalSeconds:F1} seconds";
    }

    public override Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("ScheduledTaskHostedService is shutting down...");
        return Task.CompletedTask;
    }
}
