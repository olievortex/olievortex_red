using Amazon.S3;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.Processes;

public class ImportStormEventsSpcProcess(
    ISpcProcess spc,
    ISpcBusiness spcBusiness,
    IRadarBusiness radarBusiness,
    IRadarSource radarSource)
{
    private List<RadarSiteEntity> _radarSites = [];
    private readonly List<RadarInventoryEntity> _radarInventory = [];

    public async Task RunAsync(AmazonS3Client client, CancellationToken ct)
    {
        _radarSites = await radarSource.GetPrimaryRadarSitesAsync(ct);

        foreach (var year in CommonProcess.Years) await ProcessStormReportsForYearAsync(year, client, ct);
    }

    public async Task ProcessStormReportsForYearAsync(int year, AmazonS3Client client, CancellationToken ct)
    {
        var (start, stop, inventoryYear) =
            await spc.GetInventoryByYearAsync(year, ct);
        var cutoff = DateTime.UtcNow.AddDays(-2).Date;

        for (var day = start; day <= stop; day++)
        {
            var effectiveDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(day);
            if (effectiveDate > cutoff) break;

            var inventory = await spc.SourceInventoryAsync(effectiveDate, inventoryYear, ct);
            if (spc.ShouldSkip(inventory)) continue;

            var events = spcBusiness.Parse(effectiveDate, inventory.Rows);
            await AssignRadarAsync(events, client, ct);

            await spc.ProcessEvents(events, inventory, ct);
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