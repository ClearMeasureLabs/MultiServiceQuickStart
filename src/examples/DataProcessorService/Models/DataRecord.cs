namespace DataProcessorService.Models;

/// <summary>
/// Represents a data record to be processed.
/// </summary>
public class DataRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsProcessed { get; set; }
}
