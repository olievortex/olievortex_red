using Amazon.S3;
using OlievortexRed.Lib.Entities;

namespace OlievortexRed.Lib.Radar.Interfaces;

public interface IRadarSource
{
    Task AddRadarInventoryAsync(List<RadarInventoryEntity> cache, RadarSiteEntity radar, DateTime effectiveTime,
        AmazonS3Client client, CancellationToken ct);

    Task CreateRadarSiteAsync(RadarSiteEntity entity, CancellationToken ct);
    RadarSiteEntity FindClosestRadar(List<RadarSiteEntity> radarSites, double lat, double lon);
    Task<List<RadarSiteEntity>> GetPrimaryRadarSitesAsync(CancellationToken ct);

    Task<RadarInventoryEntity?> GetRadarInventoryAsync(List<RadarInventoryEntity> cache, RadarSiteEntity radar,
        DateTime effectiveTime, CancellationToken ct);
}