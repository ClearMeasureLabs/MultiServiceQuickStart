using DataProcessorService.Models;
using Microsoft.Extensions.Logging;

namespace DataProcessorService.Services;

/// <summary>
/// Implementation of data processor that simulates processing logic.
/// </summary>
public class DataProcessor : IDataProcessor
{
    private readonly ILogger<DataProcessor> _logger;

    public DataProcessor(ILogger<DataProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessRecordAsync(DataRecord record, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing record {RecordId}: {RecordName}", record.Id, record.Name);

        // Simulate some processing work
        await Task.Delay(100, cancellationToken);

        // Simulate occasional processing logic
        var processingTime = DateTime.UtcNow - record.Timestamp;
        _logger.LogInformation(
            "Completed processing record {RecordId}. Age: {Age:F2} minutes", 
            record.Id, 
            processingTime.TotalMinutes);
    }
}
