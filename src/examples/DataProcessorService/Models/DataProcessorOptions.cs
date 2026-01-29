namespace DataProcessorService.Models;

/// <summary>
/// Configuration options for the data processor.
/// </summary>
public class DataProcessorOptions
{
    public int BatchSize { get; set; } = 10;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan ProcessingDelay { get; set; } = TimeSpan.FromSeconds(10);
}
