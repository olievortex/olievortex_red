using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents;

public class DatabaseProcess(IDatabaseBusiness business)
    : IDatabaseProcess
{
    public async Task<List<StormEventsDailySummaryEntity>> DeactivateOldSummariesAsync(string id, int year,
        string sourceFk, CancellationToken ct)
    {
        var summaries = await business.GetSummariesForDayAsync(id, year, ct);

        var deactivate = summaries.Where(w =>
                w.SourceFk != sourceFk &&
                w.IsCurrent)
            .ToList();

        foreach (var item in deactivate) await business.DeactivateSummaryAsync(item, ct);

        return summaries;
    }

    public List<DailySummaryModel> GetAggregate(List<DailyDetailModel> models)
    {
        return DailySummaryBusiness.AggregateByDate(models);
    }

    public async Task<List<DailyDetailModel>> LoadAsync(
        BlobContainerClient blobClient, StormEventsDatabaseInventoryEntity eventsDatabase, CancellationToken ct)
    {
        return await business.DatabaseLoadAsync(blobClient, eventsDatabase, ct);
    }

    public async Task SourceDatabasesAsync(BlobContainerClient blobClient, CancellationToken ct)
    {
        var eventsList = await business.DatabaseListAsync(ct);
        await business.DatabaseDownloadAsync(blobClient, eventsList, ct);
    }
}