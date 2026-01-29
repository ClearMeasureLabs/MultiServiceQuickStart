namespace ScheduledTaskService;

/// <summary>
/// Represents a schedule for task execution.
/// </summary>
public class TaskSchedule
{
    /// <summary>
    /// Gets or sets the hour (0-23) when the task should run.
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// Gets or sets the minute (0-59) when the task should run.
    /// </summary>
    public int Minute { get; set; }

    /// <summary>
    /// Gets or sets the days of week when the task should run (null means every day).
    /// </summary>
    public DayOfWeek[]? DaysOfWeek { get; set; }

    /// <summary>
    /// Calculates the next scheduled run time from now.
    /// </summary>
    public DateTime GetNextRunTime()
    {
        var now = DateTime.Now;
        var scheduledTime = DateTime.Today.AddHours(Hour).AddMinutes(Minute);

        // If the scheduled time has passed today, start from tomorrow
        if (now > scheduledTime)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        // If specific days of week are specified, find the next matching day
        if (DaysOfWeek != null && DaysOfWeek.Length > 0)
        {
            while (!DaysOfWeek.Contains(scheduledTime.DayOfWeek))
            {
                scheduledTime = scheduledTime.AddDays(1);
            }
        }

        return scheduledTime;
    }

    /// <summary>
    /// Creates a daily schedule at the specified time.
    /// </summary>
    public static TaskSchedule Daily(int hour, int minute)
    {
        return new TaskSchedule { Hour = hour, Minute = minute };
    }

    /// <summary>
    /// Creates a weekly schedule on specific days at the specified time.
    /// </summary>
    public static TaskSchedule Weekly(int hour, int minute, params DayOfWeek[] daysOfWeek)
    {
        return new TaskSchedule { Hour = hour, Minute = minute, DaysOfWeek = daysOfWeek };
    }
}
