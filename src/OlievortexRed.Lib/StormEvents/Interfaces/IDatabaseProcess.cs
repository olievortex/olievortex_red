using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents.Interfaces;

public interface IDatabaseProcess
{
    Task<List<StormEventsDailySummaryEntity>> DeactivateOldSummariesAsync(string id, int year, string sourceFk,
        CancellationToken ct);

    List<DailySummaryModel> GetAggregate(List<DailyDetailModel> models);

    Task<List<DailyDetailModel>> LoadAsync(
        BlobContainerClient blobClient, StormEventsDatabaseInventoryEntity eventsDatabase, CancellationToken ct);

    Task SourceDatabasesAsync(BlobContainerClient blobClient, CancellationToken ct);
}