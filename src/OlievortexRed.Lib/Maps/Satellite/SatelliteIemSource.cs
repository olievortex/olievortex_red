using System.Text.RegularExpressions;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Lib.Maps.Satellite;

public partial class SatelliteIemSource(IOlieWebServices ows) : ISatelliteIemSource
{
    private const string UrlBase = "https://mesonet.agron.iastate.edu/archive/data/";

    public async Task<List<string>> IemListAsync(string url, CancellationToken ct)
    {
        var html = await ows.ApiGetStringAsync(url, ct);
        var items = ItemsRegex().Matches(html);

        var result = items
            .Select(s => s.Groups[1].Value)
            .ToList();

        return result;
    }

    public int GetChannelFromKey(string value)
    {
        if (value.Contains("_ir4km_")) return 14;
        if (value.Contains("_vis4km_")) return 2;
        if (value.Contains("_wv4km_")) return 10;

        return -1;
    }

    public string GetPrefix(DateTime effectiveDate)
    {
        var dateCode = $"{effectiveDate.Year}/{effectiveDate.Month:00}/{effectiveDate.Day:00}/";
        return $"{UrlBase}{dateCode}GIS/sat/";
    }

    public DateTime GetScanTimeFromKey(DateTime effectiveDate, string value)
    {
        var filePart = Path.GetFileNameWithoutExtension(value);
        var timePart = filePart.Split('_')[3];

        var hours = int.Parse(timePart[..2]);
        var minutes = int.Parse(timePart[2..]);
        var offset = new TimeSpan(hours, minutes, 0);

        var result = effectiveDate.Add(offset);

        return result;
    }

    [GeneratedRegex("<a href=\"(\\w\\S+)\"")]
    private static partial Regex ItemsRegex();
}