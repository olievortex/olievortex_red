using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;

namespace OlievortexRed.Lib.Services;

[ExcludeFromCodeCoverage]
public class CosmosRepository(CosmosContext context) : ICosmosRepository
{
    #region RadarInventory

    public async Task RadarInventoryAddAsync(RadarInventoryEntity entity, CancellationToken ct)
    {
        await context.RadarInventory.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<RadarInventoryEntity?> RadarInventoryGetAsync(string id, string effectiveDate, string bucket,
        CancellationToken ct)
    {
        return await context.RadarInventory.SingleOrDefaultAsync(s =>
            s.Id == id &&
            s.EffectiveDate == effectiveDate &&
            s.BucketName == bucket, ct);
    }

    #endregion

    #region RadarSite

    public async Task<List<RadarSiteEntity>> RadarSiteAllAsync(CancellationToken ct)
    {
        return await context.RadarSite.ToListAsync(ct);
    }

    public async Task RadarSiteCreateAsync(RadarSiteEntity entity, CancellationToken ct)
    {
        await context.RadarSite.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region SatelliteAwsInventory

    public async Task SatelliteAwsInventoryCreateAsync(SatelliteAwsInventoryEntity entity, CancellationToken ct)
    {
        await context.SatelliteAwsInventory.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<SatelliteAwsInventoryEntity>> SatelliteAwsInventoryListByYearAsync(int year, int channel,
        DayPartsEnum dayPart, CancellationToken ct)
    {
        return await context.SatelliteAwsInventory
            .Where(w =>
                w.EffectiveDate.StartsWith($"{year}-") &&
                w.Channel == channel &&
                w.DayPart == dayPart)
            .ToListAsync(ct);
    }

    #endregion

    #region SatelliteAwsProduct

    public async Task SatelliteAwsProductCreateAsync(SatelliteAwsProductEntity entity, CancellationToken ct)
    {
        context.SatelliteAwsProduct.Add(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task<SatelliteAwsProductEntity> SatelliteAwsProductGetAsync(string id, string effectiveDate,
        CancellationToken ct)
    {
        return await context.SatelliteAwsProduct
            .Where(w => w.Id == id &&
                        w.EffectiveDate == effectiveDate)
            .SingleAsync(ct);
    }

    public async Task<SatelliteAwsProductEntity?> SatelliteAwsProductGetLastPosterAsync(string effectiveDate,
        CancellationToken ct)
    {
        return await context.SatelliteAwsProduct
            .Where(w => w.EffectiveDate == effectiveDate)
            .OrderByDescending(o => o.ScanTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SatelliteAwsProductEntity?> SatelliteAwsProductGetPosterAsync(string effectiveDate,
        DateTime eventTime, CancellationToken ct)
    {
        return await context.SatelliteAwsProduct
            .Where(w => w.EffectiveDate == effectiveDate &&
                        w.ScanTime >= eventTime)
            .OrderBy(o => o.ScanTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<SatelliteAwsProductEntity>> SatelliteAwsProductListAsync(string effectiveDate,
        string bucketName, int channel, CancellationToken ct)
    {
        return await context.SatelliteAwsProduct
            .Where(w =>
                w.EffectiveDate == effectiveDate &&
                w.BucketName == bucketName &&
                w.Channel == channel)
            .OrderBy(o => o.ScanTime)
            .ToListAsync(ct);
    }

    public async Task<List<SatelliteAwsProductEntity>> SatelliteAwsProductListNoPosterAsync(CancellationToken ct)
    {
        return await context.SatelliteAwsProduct
            .Where(w =>
                w.Path1080 != null &&
                w.PathPoster == null)
            .OrderBy(o => o.ScanTime)
            .ToListAsync(ct);
    }

    public async Task SatelliteAwsProductUpdateAsync(SatelliteAwsProductEntity entity, CancellationToken ct)
    {
        context.SatelliteAwsProduct.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region SpcMesoProduct

    public async Task SpcMesoProductCreateAsync(SpcMesoProductEntity entity, CancellationToken ct)
    {
        await context.SpcMesoProduct.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<SpcMesoProductEntity?> SpcMesoProductGetAsync(int year, int index, CancellationToken ct)
    {
        return await context.SpcMesoProduct
            .Where(w =>
                w.EffectiveDate.StartsWith($"{year}-") &&
                w.Id == index)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<SpcMesoProductEntity?> SpcMesoProductGetLatestAsync(int year, CancellationToken ct)
    {
        return await context.SpcMesoProduct
            .Where(w => w.EffectiveDate.StartsWith($"{year}-"))
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task SpcMesoProductUpdateAsync(SpcMesoProductEntity entity, CancellationToken ct)
    {
        context.SpcMesoProduct.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region StormEventsDailyDetail

    public async Task StormEventsDailyDetailCreateAsync(List<StormEventsDailyDetailEntity> entities,
        CancellationToken ct)
    {
        foreach (var entity in entities)
        {
            await context.StormEventsDailyDetail.AddAsync(entity, ct);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task StormEventsDailyDetailDeleteAsync(string dateFk, string sourceFk, CancellationToken ct)
    {
        var items = await context.StormEventsDailyDetail
            .Where(w =>
                w.DateFk == dateFk &&
                w.SourceFk == sourceFk)
            .ToListAsync(ct);

        foreach (var item in items)
        {
            context.StormEventsDailyDetail.Remove(item);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> StormEventsDailyDetailCountAsync(string dateFk, string sourceFk, CancellationToken ct)
    {
        return await context.StormEventsDailyDetail
            .CountAsync(w =>
                w.DateFk == dateFk &&
                w.SourceFk == sourceFk, ct);
    }

    #endregion

    #region StormEventsDailySummary

    public async Task StormEventsDailySummaryCreateAsync(StormEventsDailySummaryEntity entity,
        CancellationToken ct)
    {
        await context.StormEventsDailySummary.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<StormEventsDailySummaryEntity>> StormEventsDailySummaryListMissingPostersForYear(int year,
        CancellationToken ct)
    {
        return await context.StormEventsDailySummary
            .Where(w =>
                w.Year == year &&
                w.HeadlineEventTime != null &&
                // ReSharper disable once EntityFramework.UnsupportedServerSideFunctionCall
                (EF.Functions.CoalesceUndefined(w.SatellitePath1080, null) == null ||
                 // ReSharper disable once EntityFramework.UnsupportedServerSideFunctionCall
                 EF.Functions.CoalesceUndefined(w.SatellitePathPoster, null) == null))
            .ToListAsync(ct);
    }

    public async Task<List<StormEventsDailySummaryEntity>> StormEventsDailySummaryListSevereForYear(int year,
        CancellationToken ct)
    {
        return await context.StormEventsDailySummary
            .Where(w =>
                w.Year == year)
            .ToListAsync(ct);
    }

    public async Task<List<StormEventsDailySummaryEntity>> StormEventsDailySummaryListSummariesForDate(string id,
        int year, CancellationToken ct)
    {
        return await context.StormEventsDailySummary
            .Where(w =>
                w.Id == id &&
                w.Year == year)
            .ToListAsync(ct);
    }

    public async Task StormEventsDailySummaryUpdateAsync(StormEventsDailySummaryEntity entity, CancellationToken ct)
    {
        context.StormEventsDailySummary.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region StormEventsDatabaseInventory

    public async Task<List<StormEventsDatabaseInventoryEntity>> StormEventsDatabaseInventoryAllAsync(
        CancellationToken ct)
    {
        return await context.StormEventsDatabaseInventory.ToListAsync(ct);
    }

    public async Task StormEventsDatabaseInventoryCreateAsync(StormEventsDatabaseInventoryEntity entity,
        CancellationToken ct)
    {
        await context.StormEventsDatabaseInventory.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<StormEventsDatabaseInventoryEntity?> StormEventsDatabaseInventoryGetAsync(int year, string id,
        CancellationToken ct)
    {
        return await context.StormEventsDatabaseInventory.SingleOrDefaultAsync(s =>
            s.Id == id &&
            s.Year == year, ct);
    }

    public async Task StormEventsDatabaseInventoryUpdateAsync(StormEventsDatabaseInventoryEntity entity,
        CancellationToken ct)
    {
        context.StormEventsDatabaseInventory.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion

    #region StormEventsSpcInventory

    public async Task StormEventsSpcInventoryCreateAsync(StormEventsSpcInventoryEntity entity, CancellationToken ct)
    {
        await context.StormEventsSpcInventory.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<StormEventsSpcInventoryEntity>> StormEventsSpcInventoryListByYearAsync(
        int year, CancellationToken ct)
    {
        var yearValue = $"{year}-";

        return await context.StormEventsSpcInventory
            .Where(w => w.EffectiveDate.StartsWith(yearValue))
            .ToListAsync(ct);
    }

    public async Task StormEventsSpcInventoryUpdateAsync(StormEventsSpcInventoryEntity entity, CancellationToken ct)
    {
        context.StormEventsSpcInventory.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    #endregion
}