namespace OlievortexRed.Lib.Maps.Interfaces;

public interface ISatelliteIemSource
{
    Task<List<string>> IemListAsync(string url, CancellationToken ct);

    int GetChannelFromKey(string value);

    string GetPrefix(DateTime effectiveDate);

    DateTime GetScanTimeFromKey(DateTime effectiveDate, string value);
}