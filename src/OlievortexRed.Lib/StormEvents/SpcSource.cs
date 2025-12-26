using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents;

public partial class SpcSource(IOlieWebServices ows) : ISpcSource
{
    private const string StormEventsUrl = "https://www.spc.noaa.gov/climo/reports/";

    public async Task<(string body, string etag)> DownloadNewAsync(DateTime effectiveDate, CancellationToken ct)
    {
        var url = GetUrl(effectiveDate);
        var (status, etagValue, body) = await ows.ApiGetAsync(url, null, ct);
        if (status >= HttpStatusCode.Ambiguous) throw new Exception("Expected an result, got a redirect");
        if (etagValue is null) throw new Exception("Expected an etag, but it was missing");

        return (body, etagValue.Tag);
    }

    public async Task<(string body, string etag, bool isUpdated)> DownloadUpdateAsync(
        DateTime effectiveDate, string etag, CancellationToken ct)
    {
        var url = GetUrl(effectiveDate);
        var etagHeader = new EntityTagHeaderValue(etag);
        var (status, etagValue, body) = await ows.ApiGetAsync(url, etagHeader, ct);
        if (status >= HttpStatusCode.BadRequest) throw new ApplicationException($"Status {status} for {url}");
        if (status >= HttpStatusCode.Ambiguous) return (string.Empty, string.Empty, false); // Matched the etag

        if (etagValue is null) throw new ApplicationException("Expected an etag, but it was missing");

        return (body, etagValue.Tag, true);
    }

    public static string GetUrl(DateTime effectiveDate)
    {
        var filename = $"{effectiveDate:yyMMdd}_rpts_filtered.csv";
        var url = $"{StormEventsUrl}{filename}";

        return url;
    }

    public List<DailyDetailModel> Parse(DateTime effectiveDate, string[] lines)
    {
        var result = new List<DailyDetailModel>();
        var eventType = "Unknown";

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            #region Split Line

            var parts = line.Split(',', 8);
            if (parts.Length != 8) throw new Exception($"expected 8 parts: got {parts.Length} {line}");

            #endregion

            #region Section Header

            if (parts[0] == "Time")
            {
                switch (parts[1])
                {
                    case "F_Scale":
                        eventType = "Tornado";
                        break;
                    case "Speed":
                        eventType = "Thunderstorm Wind";
                        break;
                    case "Size":
                        eventType = "Hail";
                        break;
                    default:
                        throw new Exception($"unexpected magnitude field {parts[1]}");
                }

                continue;
            }

            #endregion

            var magnitude = "Unknown";
            var narrative = parts[7].Length > 6 ? parts[7][..^5].Trim() : parts[7];

            switch (eventType)
            {
                case "Hail":
                    if (parts[1] != "UNK") magnitude = $"{float.Parse(parts[1]) / 100.0:0.00}";

                    break;
                case "Thunderstorm Wind":
                    if (parts[1] != "UNK") magnitude = $"{int.Parse(parts[1])}";

                    break;
                case "Tornado":
                    var matches = TornadoRatingRegex().Matches(narrative);

                    magnitude = matches.Count > 0 ? matches[0].Value.Replace("-", string.Empty) : "EFU";

                    break;
                default:
                    throw new Exception("Unable to determine report type");
            }

            if (!float.TryParse(parts[5], out var latitude)) continue;
            if (!float.TryParse(parts[6], out var longitude)) continue;

            var model = new DailyDetailModel
            {
                Effective = ParseTime(effectiveDate, parts[0]),
                EventType = eventType,
                State = parts[4],
                Latitude = latitude,
                Longitude = longitude,
                ForecastOffice = parts[7].Length > 6 ? parts[7][^4..^1] : string.Empty,
                County = parts[3],
                City = parts[2],
                Narrative = narrative,
                Magnitude = magnitude
            };

            result.Add(model);
        }

        return result;
    }

    public static DateTime ParseTime(DateTime effectiveDate, string time)
    {
        time = time.PadLeft(4, '0');
        var hour = int.Parse(time[0..2]);
        var minute = int.Parse(time[2..4]);
        var day = hour < 12 ? 1 : 0;
        var ts = new TimeSpan(day, hour, minute, 0);

        return effectiveDate.Add(ts);
    }

    [GeneratedRegex(@"EF[-]?\d")]
    private static partial Regex TornadoRatingRegex();
}