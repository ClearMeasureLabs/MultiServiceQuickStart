using DataProcessorService.Models;
using Microsoft.Extensions.Logging;

namespace DataProcessorService.Services;

/// <summary>
/// In-memory implementation of IDataRepository for demonstration purposes.
/// </summary>
public class InMemoryDataRepository : IDataRepository
{
    private readonly List<DataRecord> _records;
    private readonly ILogger<InMemoryDataRepository> _logger;
    private static int _nextId = 1;

    public InMemoryDataRepository(ILogger<InMemoryDataRepository> logger)
    {
        _logger = logger;
        _records = GenerateTestRecords(50);
    }

    public Task<IEnumerable<DataRecord>> GetUnprocessedRecordsAsync(int batchSize, CancellationToken cancellationToken)
    {
        var unprocessed = _records
            .Where(r => !r.IsProcessed)
            .Take(batchSize)
            .ToList();

        _logger.LogDebug("Retrieved {Count} unprocessed records", unprocessed.Count);
        return Task.FromResult<IEnumerable<DataRecord>>(unprocessed);
    }

    public Task MarkAsProcessedAsync(int recordId, CancellationToken cancellationToken)
    {
        var record = _records.FirstOrDefault(r => r.Id == recordId);
        if (record != null)
        {
            record.IsProcessed = true;
            _logger.LogDebug("Marked record {RecordId} as processed", recordId);
        }
        return Task.CompletedTask;
    }

    private static List<DataRecord> GenerateTestRecords(int count)
    {
        var records = new List<DataRecord>();
        for (int i = 0; i < count; i++)
        {
            records.Add(new DataRecord
            {
                Id = _nextId++,
                Name = $"Record_{i + 1}",
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                IsProcessed = false
            });
        }
        return records;
    }
}
