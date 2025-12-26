using Amazon.S3;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.Processes;

public class ImportStormEventsDatabaseProcess(
    IDatabaseProcess database,
    IDatabaseBusiness dbBusiness,
    IRadarSource radarSource,
    IRadarBusiness radarBusiness)
{
    private List<RadarSiteEntity> _radarSites = [];
    private readonly List<RadarInventoryEntity> _radarInventory = [];
    private int _dayCount;

    public async Task<bool> RunAsync(int year, string id, BlobContainerClient blobClient, AmazonS3Client amazonClient,
        CancellationToken ct)
    {
        _radarSites = await radarSource.GetPrimaryRadarSitesAsync(ct);
        _radarInventory.Clear();
        _dayCount = 0;

        await database.SourceDatabasesAsync(blobClient, ct);
        return await ProcessEventsDatabasesAsync(year, id, blobClient, amazonClient, ct);
    }

    private async Task<bool> ProcessEventsDatabasesAsync(int year, string id, BlobContainerClient blobClient,
        AmazonS3Client amazonClient, CancellationToken ct)
    {
        var inventory = await dbBusiness.DatabaseGetInventoryAsync(year, id, ct)
                        ?? throw new InvalidOperationException($"No record for year {year}, sourceFk {id}");

        var events = await database.LoadAsync(blobClient, inventory, ct);

        if (inventory.RowCount == 0)
        {
            await dbBusiness.DatabaseUpdateRowCountAsync(inventory, events.Count, ct);
            inventory.RowCount = events.Count;
        }

        var start = new DateTime(year, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var work = events
            .Where(w => w.Effective >= start)
            .Select(s => s.EffectiveDate)
            .Distinct()
            .OrderBy(o => o)
            .ToList();

        foreach (var workItem in work)
        {
            var toProcess = events.Where(w => w.EffectiveDate == workItem).ToList();
            await ProcessWorkItemAsync(workItem, year, id, toProcess, amazonClient, ct);

            if (_dayCount > 31) return true;
        }

        var result = _dayCount > 0;

        if (!result) await dbBusiness.DatabaseUpdateActiveAsync(inventory, ct);

        return result;
    }

    private async Task ProcessWorkItemAsync(string id, int year, string sourceFk, List<DailyDetailModel> models,
        AmazonS3Client amazonClient, CancellationToken ct)
    {
        Console.WriteLine($"Id: {id}, SourceFk: {sourceFk}");

        var summaries = await database.DeactivateOldSummariesAsync(id, year, sourceFk, ct);
        var current = summaries.SingleOrDefault(s => s.SourceFk == sourceFk);

        if (current is not null && !current.IsCurrent)
        {
            await dbBusiness.CompareDetailCountAsync(id, sourceFk, current.RowCount, ct);
            await dbBusiness.ActivateSummaryAsync(current, ct);
            if (_dayCount == 0) _dayCount++;
        }
        else if (current is null)
        {
            await dbBusiness.DeleteDetailAsync(id, sourceFk, ct);
            await AssignRadarAsync(models, amazonClient, ct);
            await dbBusiness.AddDailyDetailToCosmosAsync(models, sourceFk, ct);
            var aggregate = database.GetAggregate(models);
            if (aggregate.Count != 1)
                throw new InvalidOperationException(
                    $"Got more than one aggregate for dateFk: {id}, sourceFk: {sourceFk}");
            await dbBusiness.AddDailySummaryToCosmosAsync(aggregate[0], sourceFk, ct);
            _dayCount++;
        }
    }

    private async Task AssignRadarAsync(List<DailyDetailModel> stormEvents, AmazonS3Client client, CancellationToken ct)
    {
        foreach (var stormEvent in stormEvents)
        {
            var radarSite = await radarBusiness.DownloadInventoryForClosestRadarAsync(_radarSites, _radarInventory,
                stormEvent.Effective, stormEvent.Latitude, stormEvent.Longitude, client, ct);
            stormEvent.ClosestRadar = radarSite.Id;
        }
    }
}