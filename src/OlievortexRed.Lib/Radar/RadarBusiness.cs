using Amazon.S3;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Radar.Interfaces;

namespace OlievortexRed.Lib.Radar;

public class RadarBusiness(IRadarSource source) : IRadarBusiness
{
    public async Task<RadarSiteEntity> DownloadInventoryForClosestRadarAsync(List<RadarSiteEntity> radarSites,
        List<RadarInventoryEntity> cache, DateTime effectiveTime, double latitude, double longitude,
        AmazonS3Client client, CancellationToken ct)
    {
        var radarSite = source.FindClosestRadar(radarSites, latitude, longitude);
        await DownloadInventory(effectiveTime);
        await DownloadInventory(effectiveTime.AddHours(-3));
        await DownloadInventory(effectiveTime.AddHours(1));

        return radarSite;

        async Task DownloadInventory(DateTime timeValue)
        {
            var inventory = await source.GetRadarInventoryAsync(cache, radarSite, timeValue, ct);
            if (inventory is null) await source.AddRadarInventoryAsync(cache, radarSite, timeValue, client, ct);
        }
    }

    public async Task PopulateRadarSitesFromCsvAsync(string csv, CancellationToken ct)
    {
        var lines = csv.ReplaceLineEndings("\n").Split('\n');
        var lineNumber = 0;

        foreach (var line in lines)
        {
            // Skip the header
            lineNumber++;
            if (lineNumber < 3) continue;
            if (string.IsNullOrEmpty(line)) continue;

            // Parse the parts
            var icao = line[9..13];
            var name = line[20..50].Trim();
            var state = line[72..74].Trim();
            var lat = double.Parse(line[106..115]);
            var lon = double.Parse(line[116..126]);
            if (string.IsNullOrWhiteSpace(state)) continue;

            // Create the record
            var entity = new RadarSiteEntity
            {
                Id = icao,

                Name = name,
                State = state,
                Latitude = lat,
                Longitude = lon,
                Timestamp = DateTime.UtcNow
            };

            await source.CreateRadarSiteAsync(entity, ct);
        }
    }
}