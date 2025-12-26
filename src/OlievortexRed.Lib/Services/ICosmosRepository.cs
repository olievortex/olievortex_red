using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;

namespace OlievortexRed.Lib.Services;

public interface ICosmosRepository
{
    #region RadarInventory

    Task RadarInventoryAddAsync(RadarInventoryEntity entity, CancellationToken ct);

    Task<RadarInventoryEntity?> RadarInventoryGetAsync(string id, string effectiveDate, string bucket,
        CancellationToken ct);

    #endregion

    #region RadarSite

    Task<List<RadarSiteEntity>> RadarSiteAllAsync(CancellationToken ct);

    Task RadarSiteCreateAsync(RadarSiteEntity entity, CancellationToken ct);

    #endregion

    #region SatelliteAwsInventory

    Task SatelliteAwsInventoryCreateAsync(SatelliteAwsInventoryEntity entity, CancellationToken ct);

    Task<List<SatelliteAwsInventoryEntity>> SatelliteAwsInventoryListByYearAsync(int year, int channel,
        DayPartsEnum dayPart, CancellationToken ct);

    #endregion

    #region SatelliteAwsProduct

    Task SatelliteAwsProductCreateAsync(SatelliteAwsProductEntity entity, CancellationToken ct);
    Task<SatelliteAwsProductEntity> SatelliteAwsProductGetAsync(string id, string effectiveDate, CancellationToken ct);

    Task<SatelliteAwsProductEntity?> SatelliteAwsProductGetLastPosterAsync(string effectiveDate,
        CancellationToken ct);

    Task<SatelliteAwsProductEntity?> SatelliteAwsProductGetPosterAsync(string effectiveDate, DateTime eventTime,
        CancellationToken ct);

    Task<List<SatelliteAwsProductEntity>> SatelliteAwsProductListAsync(string effectiveDate, string bucketName,
        int channel, CancellationToken ct);

    Task<List<SatelliteAwsProductEntity>> SatelliteAwsProductListNoPosterAsync(CancellationToken ct);

    Task SatelliteAwsProductUpdateAsync(SatelliteAwsProductEntity entity, CancellationToken ct);

    #endregion

    #region SpcMesoProduct

    Task SpcMesoProductCreateAsync(SpcMesoProductEntity entity, CancellationToken ct);

    Task<SpcMesoProductEntity?> SpcMesoProductGetAsync(int year, int index, CancellationToken ct);

    Task<SpcMesoProductEntity?> SpcMesoProductGetLatestAsync(int year, CancellationToken ct);

    Task SpcMesoProductUpdateAsync(SpcMesoProductEntity entity, CancellationToken ct);

    #endregion

    #region StormEventsDailyDetail

    Task StormEventsDailyDetailCreateAsync(List<StormEventsDailyDetailEntity> entities, CancellationToken ct);

    Task StormEventsDailyDetailDeleteAsync(string dateFk, string sourceFk, CancellationToken ct);

    Task<int> StormEventsDailyDetailCountAsync(string dateFk, string sourceFk, CancellationToken ct);

    #endregion

    #region StormEventsDailySummary

    Task StormEventsDailySummaryCreateAsync(StormEventsDailySummaryEntity entity, CancellationToken ct);

    Task<List<StormEventsDailySummaryEntity>> StormEventsDailySummaryListMissingPostersForYear(int year,
        CancellationToken ct);

    Task<List<StormEventsDailySummaryEntity>> StormEventsDailySummaryListSevereForYear(int year,
        CancellationToken ct);

    Task<List<StormEventsDailySummaryEntity>> StormEventsDailySummaryListSummariesForDate(string id, int year,
        CancellationToken ct);

    Task StormEventsDailySummaryUpdateAsync(StormEventsDailySummaryEntity entity, CancellationToken ct);

    #endregion

    #region StormEventsDatabaseInventory

    Task<List<StormEventsDatabaseInventoryEntity>> StormEventsDatabaseInventoryAllAsync(CancellationToken ct);

    Task StormEventsDatabaseInventoryCreateAsync(StormEventsDatabaseInventoryEntity entity, CancellationToken ct);

    Task<StormEventsDatabaseInventoryEntity?> StormEventsDatabaseInventoryGetAsync(int year, string id,
        CancellationToken ct);

    Task StormEventsDatabaseInventoryUpdateAsync(StormEventsDatabaseInventoryEntity entity, CancellationToken ct);

    #endregion

    #region StormEventsSpcInventory

    Task StormEventsSpcInventoryCreateAsync(StormEventsSpcInventoryEntity entity, CancellationToken ct);

    Task<List<StormEventsSpcInventoryEntity>> StormEventsSpcInventoryListByYearAsync(int year, CancellationToken ct);

    Task StormEventsSpcInventoryUpdateAsync(StormEventsSpcInventoryEntity entity, CancellationToken ct);

    #endregion
}