using System.Globalization;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents;

public class DailySummaryBusiness(ICosmosRepository cosmos) : IDailySummaryBusiness
{
    public static List<DailySummaryModel> AggregateByDate(List<DailyDetailModel> events)
    {
        if (events.Any(a => a.Effective.Kind != DateTimeKind.Utc))
            throw new ArgumentException("DateTimes need to be UTC");

        var agg = events.Select(s => new
        {
            s.EffectiveDate,
            Hail = s.EventType == "Hail" ? 1 : 0,
            Wind = s.EventType == "Thunderstorm Wind" ? 1 : 0,
            F5 = s is { EventType: "Tornado", Magnitude: "EF5" } ? 1 : 0,
            F4 = s is { EventType: "Tornado", Magnitude: "EF4" } ? 1 : 0,
            F3 = s is { EventType: "Tornado", Magnitude: "EF3" } ? 1 : 0,
            F2 = s is { EventType: "Tornado", Magnitude: "EF2" } ? 1 : 0,
            F1 = s is { EventType: "Tornado", Magnitude: "EF1" or "EF0" or "EFU" } ? 1 : 0,
            HeadlineScore = EncodeHeadlineScore(s)
        });

        var result = agg.GroupBy(g => g.EffectiveDate)
            .Select(g => new DailySummaryModel
            {
                EffectiveDate = g.Key,
                Hail = g.Sum(s => s.Hail),
                Wind = g.Sum(s => s.Wind),
                F5 = g.Sum(s => s.F5),
                F4 = g.Sum(s => s.F4),
                F3 = g.Sum(s => s.F3),
                F2 = g.Sum(s => s.F2),
                F1 = g.Sum(s => s.F1),
                HeadlineEventTime = DecodeHeadlineScore(g.Min(m => m.HeadlineScore))
            })
            .OrderBy(o => o.EffectiveDate)
            .ToList();

        return result;
    }

    public static DateTime? DecodeHeadlineScore(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith('9')) return null;

        var effective = DateTime.ParseExact(value[1..], "u", CultureInfo.CurrentCulture);

        return effective;
    }

    public static string EncodeHeadlineScore(DailyDetailModel model)
    {
        if (model.Effective.TimeOfDay.Hours < 18) return "9";

        var prefix = model.EventType switch
        {
            "Hail" => "7",
            "Thunderstorm Wind" => "6",
            "Tornado" => model.Magnitude switch
            {
                "EF5" => "0",
                "EF4" => "2",
                "EF3" => "2",
                "EF2" => "3",
                "EF1" => "4",
                _ => "5"
            },
            _ => "8"
        };

        return $"{prefix}{model.Effective:u}";
    }

    public async Task<List<StormEventsDailySummaryEntity>> GetMissingPostersByYearAsync(int year, CancellationToken ct)
    {
        return await cosmos.StormEventsDailySummaryListMissingPostersForYear(year, ct);
    }

    public async Task<List<StormEventsDailySummaryEntity>> GetSevereByYearAsync(int year, CancellationToken ct)
    {
        var entities = await cosmos.StormEventsDailySummaryListSevereForYear(year, ct);

        var selector = entities
            .GroupBy(g => new
            {
                g.Id,
                g.Year
            })
            .Select(s => new
            {
                s.Key.Id,
                s.Key.Year,
                Timestamp = s.Max(m => m.Timestamp)
            })
            .ToList();

        var top = entities
            .Join(selector,
                o => new { o.Id, o.Year, o.Timestamp },
                i => i,
                (o, _) => o)
            .ToList();

        return top;
    }

    public async Task UpdateCosmosAsync(StormEventsDailySummaryEntity stormSummary, CancellationToken ct)
    {
        await cosmos.StormEventsDailySummaryUpdateAsync(stormSummary, ct);
    }
}