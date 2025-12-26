using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents.Interfaces;

public interface IDatabaseBusiness
{
    #region Detail

    Task AddDailyDetailToCosmosAsync(List<DailyDetailModel> models, string sourceFk, CancellationToken ct);

    Task CompareDetailCountAsync(string dateFk, string sourceFk, int count, CancellationToken ct);

    Task DeleteDetailAsync(string dateFk, string sourceFk, CancellationToken ct);

    #endregion

    #region Summary

    Task ActivateSummaryAsync(StormEventsDailySummaryEntity entity, CancellationToken ct);

    Task AddDailySummaryToCosmosAsync(DailySummaryModel model, string sourceFk, CancellationToken ct);

    Task DeactivateSummaryAsync(StormEventsDailySummaryEntity entity, CancellationToken ct);

    Task<List<StormEventsDailySummaryEntity>> GetSummariesForDayAsync(string id, int year,
        CancellationToken ct);

    #endregion

    #region Database

    Task DatabaseDownloadAsync(BlobContainerClient client, List<DatabaseFileModel> model, CancellationToken ct);

    Task<StormEventsDatabaseInventoryEntity?> DatabaseGetInventoryAsync(int year, string id, CancellationToken ct);

    Task<List<DatabaseFileModel>> DatabaseListAsync(CancellationToken ct);

    Task<List<DailyDetailModel>> DatabaseLoadAsync(
        BlobContainerClient blobClient, StormEventsDatabaseInventoryEntity eventsDatabase, CancellationToken ct);

    Task DatabaseUpdateActiveAsync(StormEventsDatabaseInventoryEntity entity, CancellationToken ct);

    Task DatabaseUpdateRowCountAsync(StormEventsDatabaseInventoryEntity entity, int rowCount, CancellationToken ct);

    #endregion
}