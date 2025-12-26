using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Mapping;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents;

public class SpcBusiness(ICosmosRepository cosmos, ISpcSource source) : ISpcBusiness
{
    public async Task AddDailyDetailAsync(List<DailyDetailModel> models,
        StormEventsSpcInventoryEntity inventory, CancellationToken ct)
    {
        if (inventory.IsDailyDetailComplete) return;

        await cosmos.StormEventsDailyDetailDeleteAsync(inventory.EffectiveDate, inventory.Id, ct);
        var entities = EntityMapping.ToStormEventsDailyDetail(models, inventory.Id);
        await cosmos.StormEventsDailyDetailCreateAsync(entities, ct);

        inventory.IsDailyDetailComplete = true;
        inventory.Timestamp = DateTime.UtcNow;
        await cosmos.StormEventsSpcInventoryUpdateAsync(inventory, ct);
    }

    public async Task AddDailySummaryAsync(StormEventsSpcInventoryEntity inventory, DailySummaryModel? model,
        string sourceFk, CancellationToken ct)
    {
        if (inventory.IsDailySummaryComplete) return;
        if (model is null) return;

        foreach (var oldInventory in await cosmos.StormEventsDailySummaryListSummariesForDate(inventory.EffectiveDate,
                     inventory.DecodeEffectiveDate().Year, ct))
            if (oldInventory.IsCurrent)
            {
                oldInventory.IsCurrent = false;
                oldInventory.Timestamp = DateTime.UtcNow;
                await cosmos.StormEventsDailySummaryUpdateAsync(oldInventory, ct);
            }

        var entity = EntityMapping.ToStormEventsDailySummary([model], sourceFk)[0];
        entity.IsCurrent = true;
        await cosmos.StormEventsDailySummaryCreateAsync(entity, ct);

        var tornadoes = entity.F1 + entity.F2 + entity.F3 + entity.F4 + entity.F5;
        inventory.IsDailySummaryComplete = true;
        inventory.IsTornadoDay = tornadoes > 0;
        await cosmos.StormEventsSpcInventoryUpdateAsync(inventory, ct);
    }

    public async Task<StormEventsSpcInventoryEntity> DownloadNewAsync(DateTime effectiveDate, CancellationToken ct)
    {
        var (body, etag) = await source.DownloadNewAsync(effectiveDate, ct);
        var entity = StormEventsSpcInventoryEntity.FromValues(effectiveDate, body, etag);

        await cosmos.StormEventsSpcInventoryCreateAsync(entity, ct);

        return entity;
    }

    public async Task<StormEventsSpcInventoryEntity> DownloadUpdateAsync(StormEventsSpcInventoryEntity inventory,
        CancellationToken ct)
    {
        if ((DateTime.UtcNow - inventory.Timestamp).TotalDays < 8) return inventory;
        if (!inventory.IsTornadoDay) return inventory;

        var (body, etag, isUpdated) =
            await source.DownloadUpdateAsync(inventory.DecodeEffectiveDate(), inventory.Id, ct);
        if (!isUpdated)
        {
            inventory.Timestamp = DateTime.UtcNow;
            await cosmos.StormEventsSpcInventoryUpdateAsync(inventory, ct);
            return inventory; // Matched the etag
        }

        var entity = StormEventsSpcInventoryEntity.FromValues(inventory.DecodeEffectiveDate(), body, etag);
        await cosmos.StormEventsSpcInventoryCreateAsync(entity, ct);

        return entity;
    }

    public DailySummaryModel? GetAggregate(List<DailyDetailModel> models)
    {
        var aggregate = DailySummaryBusiness.AggregateByDate(models);
        if (aggregate.Count > 1) throw new Exception("SPC Storm Reports day misalignment");
        if (aggregate.Count == 0) return null;

        return aggregate[0];
    }

    public StormEventsSpcInventoryEntity? GetLatest(DateTime effectiveDate,
        List<StormEventsSpcInventoryEntity> inventory)
    {
        return inventory
            .Where(w => w.EffectiveDate == $"{effectiveDate:yyyy-MM-dd}")
            .OrderByDescending(o => o.Timestamp)
            .FirstOrDefault();
    }

    public List<DailyDetailModel> Parse(DateTime effectiveDate, string[] lines)
    {
        return source.Parse(effectiveDate, lines);
    }

    public static int GetFirstDayNumberForYear(int year)
    {
        return year switch
        {
            < 2025 => int.MaxValue,
            // 2024 => (int)(new DateTime(2024, 12, 1) - new DateTime(2024, 1, 1)).TotalDays,
            _ => 0
        };
    }

    public static int GetLastDayNumberForYear(int year)
    {
        return (int)(new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).TotalDays;
    }

    public async Task<List<StormEventsSpcInventoryEntity>> GetInventoryByYearAsync(int year, CancellationToken ct)
    {
        return await cosmos.StormEventsSpcInventoryListByYearAsync(year, ct);
    }
}