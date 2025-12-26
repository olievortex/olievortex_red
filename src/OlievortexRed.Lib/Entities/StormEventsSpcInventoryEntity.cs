using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OlievortexRed.Lib.Entities;

public class StormEventsSpcInventoryEntity
{
    [MaxLength(320)] public string Id { get; init; } = string.Empty;
    [MaxLength(32)] public string EffectiveDate { get; init; } = string.Empty; // Partition Key cannot be DateTime

    public string[] Rows { get; init; } = [];
    public DateTime Timestamp { get; set; }
    public bool IsDailySummaryComplete { get; set; }
    public bool IsDailyDetailComplete { get; set; }
    public bool IsTornadoDay { get; set; }

    public static StormEventsSpcInventoryEntity FromValues(DateTime effectiveDate, string body, string etag)
    {
        return new StormEventsSpcInventoryEntity
        {
            Id = etag,
            EffectiveDate = effectiveDate.ToString("yyyy-MM-dd"),
            IsDailySummaryComplete = false,
            IsTornadoDay = false,
            Rows = body.ReplaceLineEndings("\n").Split("\n"),
            Timestamp = DateTime.UtcNow
        };
    }

    public DateTime DecodeEffectiveDate()
    {
        return DateTime.ParseExact(EffectiveDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}