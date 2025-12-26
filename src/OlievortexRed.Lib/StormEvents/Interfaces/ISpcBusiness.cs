using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents.Interfaces;

public interface ISpcBusiness
{
    Task AddDailyDetailAsync(List<DailyDetailModel> models,
        StormEventsSpcInventoryEntity inventory, CancellationToken ct);

    Task AddDailySummaryAsync(StormEventsSpcInventoryEntity inventory, DailySummaryModel? model,
        string sourceFk, CancellationToken ct);

    Task<StormEventsSpcInventoryEntity> DownloadNewAsync(DateTime effectiveDate, CancellationToken ct);

    Task<StormEventsSpcInventoryEntity> DownloadUpdateAsync(StormEventsSpcInventoryEntity inventory,
        CancellationToken ct);

    DailySummaryModel? GetAggregate(List<DailyDetailModel> models);

    List<DailyDetailModel> Parse(DateTime effectiveDate, string[] lines);

    Task<List<StormEventsSpcInventoryEntity>> GetInventoryByYearAsync(int year, CancellationToken ct);
    StormEventsSpcInventoryEntity? GetLatest(DateTime effectiveDate, List<StormEventsSpcInventoryEntity> inventory);
}