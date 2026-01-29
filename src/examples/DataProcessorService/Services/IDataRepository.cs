using DataProcessorService.Models;

namespace DataProcessorService.Services;

/// <summary>
/// Interface for data repository operations.
/// </summary>
public interface IDataRepository
{
    /// <summary>
    /// Fetches unprocessed data records.
    /// </summary>
    Task<IEnumerable<DataRecord>> GetUnprocessedRecordsAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a record as processed.
    /// </summary>
    Task MarkAsProcessedAsync(int recordId, CancellationToken cancellationToken);
}
