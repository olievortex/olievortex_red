using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents.Interfaces;

public interface ISpcSource
{
    Task<(string body, string etag)> DownloadNewAsync(DateTime effectiveDate, CancellationToken ct);

    Task<(string body, string etag, bool isUpdated)> DownloadUpdateAsync(DateTime effectiveDate, string etag,
        CancellationToken ct);

    List<DailyDetailModel> Parse(DateTime effectiveDate, string[] lines);
}