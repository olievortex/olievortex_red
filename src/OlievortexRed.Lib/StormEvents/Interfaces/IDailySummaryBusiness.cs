using OlievortexRed.Lib.Entities;

namespace OlievortexRed.Lib.StormEvents.Interfaces;

public interface IDailySummaryBusiness
{
    Task<List<StormEventsDailySummaryEntity>> GetMissingPostersByYearAsync(int year, CancellationToken ct);

    Task<List<StormEventsDailySummaryEntity>> GetSevereByYearAsync(int year, CancellationToken ct);

    Task UpdateCosmosAsync(StormEventsDailySummaryEntity stormSummary, CancellationToken ct);
}