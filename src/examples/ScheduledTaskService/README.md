# Scheduled Task Service Example

This example demonstrates how to build a time-based scheduled task service using QuickHostedService.

## What It Does

The `ScheduledTaskHostedService`:
- Executes tasks on a specific schedule (daily, weekly, etc.)
- Calculates the next run time dynamically
- Supports day-of-week filtering
- Executes multiple tasks in sequence
- Logs detailed execution information

## Key Concepts Demonstrated

1. **Time-Based Scheduling**: Running tasks at specific times
2. **Dynamic Scheduling**: Calculating next execution time
3. **Multiple Tasks**: Orchestrating several tasks in sequence
4. **Error Recovery**: Continuing to next schedule even if a task fails
5. **Detailed Logging**: Comprehensive execution tracking

## Running the Example

```bash
cd examples/ScheduledTaskService
dotnet run
```

**Note**: For demo purposes, the task is scheduled to run every minute. In production, you would schedule it for specific times like 2:00 AM.

Press `Ctrl+C` to stop gracefully.

## Expected Output

```
===========================================
Scheduled Task Service Example
===========================================
This example demonstrates:
- Time-based task scheduling
- Calculating next run time
- Daily/weekly scheduling
- Multiple task execution

Press Ctrl+C to stop.
===========================================

[INF] Starting hosted service: ScheduledTaskHostedService
[INF] ScheduledTaskHostedService initializing...
[INF] Task scheduled for 14:30
[INF] Running daily
[INF] Hosted service started successfully: ScheduledTaskHostedService
[INF] ScheduledTaskHostedService started. Waiting for scheduled time...
[INF] Next execution scheduled for 2025-01-15 14:30:00 (in 5.2 minutes)
... (waiting)
[INF] ========================================
[INF] Executing scheduled task at 2025-01-15 14:30:00
[INF] ========================================
[INF] Generating daily report...
[INF] Report generated: DailyReport_20250115.pdf
[INF] Cleaning up old data...
[INF] Cleaned up 47 old records
[INF] Sending notifications...
[INF] Sent 12 notifications
[INF] ========================================
[INF] Scheduled task completed successfully in 2.01 seconds
[INF] ========================================
[INF] Next execution scheduled for 2025-01-16 14:30:00 (in 23.9 hours)
```

## Code Highlights

### Schedule Configuration

```csharp
// Daily at 2:00 AM
var schedule = TaskSchedule.Daily(hour: 2, minute: 0);

// Weekly on Monday and Friday at 9:00 AM
var schedule = TaskSchedule.Weekly(
    hour: 9, 
    minute: 0, 
    DayOfWeek.Monday, 
    DayOfWeek.Friday);
```

### Calculating Next Run Time

```csharp
var nextRun = _schedule.GetNextRunTime();
var now = DateTime.Now;
var delay = nextRun - now;

await Task.Delay(delay, stoppingToken);
await ExecuteScheduledTaskAsync(stoppingToken);
```

### Executing Multiple Tasks

```csharp
// Run tasks in sequence
await GenerateReportAsync(cancellationToken);
await CleanupOldDataAsync(cancellationToken);
await SendNotificationsAsync(cancellationToken);
```

## Customization

### Custom Schedule

You can easily customize the schedule:

```csharp
public ScheduledTaskHostedService()
{
    // Daily at 3:00 AM
    _schedule = TaskSchedule.Daily(3, 0);
    
    // Or weekdays only at 6:00 PM
    _schedule = TaskSchedule.Weekly(18, 0,
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday);
}
```

### Custom Tasks

Replace the example tasks with your own:

```csharp
private async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
{
    Logger.Information("Starting scheduled maintenance...");
    
    // Your custom task
    await BackupDatabaseAsync(cancellationToken);
    await ArchiveOldFilesAsync(cancellationToken);
    await SendDailyReportAsync(cancellationToken);
    
    Logger.Information("Scheduled maintenance completed");
}
```

## Real-World Use Cases

This pattern is perfect for:
- **Daily Reports**: Generate and email reports at specific times
- **Database Maintenance**: Backup, vacuum, or optimize databases
- **Data Archival**: Move old data to cold storage
- **Batch Processing**: Process accumulated data at off-peak hours
- **Cache Warming**: Pre-load caches before peak traffic
- **Cleanup Jobs**: Delete temporary files, expired sessions, etc.
- **Health Checks**: Verify system health at regular intervals
- **Data Synchronization**: Sync data between systems periodically

## Advanced Scheduling

For more complex scheduling needs, consider:

1. **Cron-like Expressions**: Integrate with a cron parser library
2. **Multiple Schedules**: Run different tasks at different times
3. **Configuration-Based**: Load schedules from configuration files
4. **Dynamic Scheduling**: Adjust schedules based on workload

## Comparison with Alternatives

| Feature | This Approach | Cron Job | Hangfire | Quartz.NET |
|---------|--------------|----------|----------|------------|
| Simplicity | ????? | ???? | ??? | ?? |
| .NET Integration | ????? | ?? | ????? | ????? |
| No Dependencies | ????? | ????? | ?? | ?? |
| Advanced Features | ?? | ??? | ????? | ????? |
| Cross-Platform | ????? | ??? | ????? | ????? |

## Next Steps

See other examples for:
- [DataProcessorService](../DataProcessorService) - Batch data processing
- [ResilientService](../ResilientService) - Error handling and retries
- [SimpleWorker](../SimpleWorker) - Basic continuous operation
