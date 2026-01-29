using DataProcessorService.Models;

namespace DataProcessorService.Services;

/// <summary>
/// Interface for processing data records.
/// </summary>
public interface IDataProcessor
{
    /// <summary>
    /// Processes a single data record.
    /// </summary>
    Task ProcessRecordAsync(DataRecord record, CancellationToken cancellationToken);
}
