using Amazon.S3;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Lib.Radar;

public class RadarSource(ICosmosRepository cosmos, IOlieWebServices ows) : IRadarSource
{
    public const string LevelIiBucket = "noaa-nexrad-level2";

    public async Task AddRadarInventoryAsync(List<RadarInventoryEntity> cache, RadarSiteEntity radar,
        DateTime effectiveTime, AmazonS3Client client, CancellationToken ct)
    {
        var prefix = $"{effectiveTime:yyyy}/{effectiveTime:MM}/{effectiveTime:dd}/{radar.Id}/";
        var products = await ows.AwsListAsync(LevelIiBucket, prefix, client, ct);

        var entity = new RadarInventoryEntity
        {
            BucketName = LevelIiBucket,
            EffectiveDate = $"{effectiveTime:yyyy-MM-dd}",
            FileList = products,
            Id = radar.Id,
            Timestamp = DateTime.UtcNow
        };

        await cosmos.RadarInventoryAddAsync(entity, ct);
        cache.Add(entity);
    }

    public async Task CreateRadarSiteAsync(RadarSiteEntity entity, CancellationToken ct)
    {
        await cosmos.RadarSiteCreateAsync(entity, ct);
    }

    public RadarSiteEntity FindClosestRadar(List<RadarSiteEntity> radarSites, double lat, double lon)
    {
        var closest = radarSites.Select(s => new
            {
                s.Id,
                Distance = Math.Sqrt(Math.Pow(s.Latitude - lat, 2) + Math.Pow(s.Longitude - lon, 2))
            })
            .OrderBy(o => o.Distance)
            .First();

        return radarSites
            .Single(s => s.Id == closest.Id);
    }

    public async Task<List<RadarSiteEntity>> GetPrimaryRadarSitesAsync(CancellationToken ct)
    {
        return (await cosmos.RadarSiteAllAsync(ct))
            .Where(w => w.Id.StartsWith("K"))
            .ToList();
    }

    public async Task<RadarInventoryEntity?> GetRadarInventoryAsync(List<RadarInventoryEntity> cache,
        RadarSiteEntity radar, DateTime effectiveTime, CancellationToken ct)
    {
        var effectiveDate = $"{effectiveTime:yyyy-MM-dd}";
        var result = cache.SingleOrDefault(a => a.BucketName == LevelIiBucket &&
                                                a.EffectiveDate == effectiveDate &&
                                                a.Id == radar.Id);

        if (result is not null) return result;

        result = await cosmos.RadarInventoryGetAsync(radar.Id, effectiveDate, LevelIiBucket, ct);

        if (result is null) return null;

        cache.Add(result);

        return result;
    }
}