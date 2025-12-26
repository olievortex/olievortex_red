using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents.Interfaces;

public interface ISpcProcess
{
    Task<(int start, int stop, List<StormEventsSpcInventoryEntity>)> GetInventoryByYearAsync(int year,
        CancellationToken ct);

    Task ProcessEvents(List<DailyDetailModel> events, StormEventsSpcInventoryEntity inventory, CancellationToken ct);
    bool ShouldSkip(StormEventsSpcInventoryEntity inventory);

    Task<StormEventsSpcInventoryEntity> SourceInventoryAsync(DateTime effectiveDate,
        List<StormEventsSpcInventoryEntity> inventoryList, CancellationToken ct);
}