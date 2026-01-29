using DataProcessorService.Models;
using DataProcessorService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuickHostedService.Application;

namespace DataProcessorService;

/// <summary>
/// A data processor service that demonstrates dependency injection and scoped services.
/// </summary>
public class DataProcessorHostedService : BaseHostedService
{
    protected override void RegisterDependencyInjection(IServiceCollection services)
    {
        // Register configuration
        services.Configure<DataProcessorOptions>(options =>
        {
            options.BatchSize = 5;
            options.MaxRetries = 3;
            options.ProcessingDelay = TimeSpan.FromSeconds(10);
        });

        // Register services with appropriate lifetimes
        services.AddSingleton<IDataRepository, InMemoryDataRepository>();
        services.AddScoped<IDataProcessor, DataProcessor>();
    }

    public override async Task OnStartingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("DataProcessorHostedService is initializing...");

        // Validate configuration
        var options = ServiceProvider.GetRequiredService<IOptions<DataProcessorOptions>>().Value;
        Logger.Information(
            "Configuration: BatchSize={BatchSize}, MaxRetries={MaxRetries}, ProcessingDelay={ProcessingDelay}",
            options.BatchSize,
            options.MaxRetries,
            options.ProcessingDelay);

        await Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.Information("DataProcessorHostedService started. Beginning data processing loop...");

        var options = ServiceProvider.GetRequiredService<IOptions<DataProcessorOptions>>().Value;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessBatchAsync(options, stoppingToken);

                // Wait before processing next batch
                Logger.Information("Waiting {Delay} before next batch...", options.ProcessingDelay);
                await Task.Delay(options.ProcessingDelay, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Information("Data processing was cancelled");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "An error occurred during data processing");
            throw;
        }
    }

    private async Task ProcessBatchAsync(DataProcessorOptions options, CancellationToken cancellationToken)
    {
        // Create a new scope for this batch
        using var scope = ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        var processor = scope.ServiceProvider.GetRequiredService<IDataProcessor>();

        // Fetch unprocessed records
        var records = await repository.GetUnprocessedRecordsAsync(options.BatchSize, cancellationToken);
        var recordsList = records.ToList();

        if (!recordsList.Any())
        {
            Logger.Information("No unprocessed records found");
            return;
        }

        Logger.Information("Processing batch of {Count} records", recordsList.Count);

        // Process each record
        foreach (var record in recordsList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await processor.ProcessRecordAsync(record, cancellationToken);
                await repository.MarkAsProcessedAsync(record.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to process record {RecordId}", record.Id);
                // Continue processing other records
            }
        }

        Logger.Information("Completed batch processing. Processed {Count} records", recordsList.Count);
    }

    public override Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        Logger.Information("DataProcessorHostedService is shutting down...");
        return Task.CompletedTask;
    }
}
