using System.ComponentModel.DataAnnotations;

namespace OlievortexRed.Lib.Entities;

public class StormEventsDailySummaryEntity
{
    [MaxLength(36)] public string Id { get; init; } = string.Empty;
    public int Year { get; init; }
    [MaxLength(36)] public string SourceFk { get; init; } = string.Empty;

    public DateTime? HeadlineEventTime { get; init; }
    [MaxLength(320)] public string? SatellitePathPoster { get; set; }
    [MaxLength(320)] public string? SatellitePath1080 { get; set; }
    public int Hail { get; init; }
    public int Wind { get; init; }
    public int F5 { get; init; }
    public int F4 { get; init; }
    public int F3 { get; init; }
    public int F2 { get; init; }
    public int F1 { get; init; }
    public int RowCount { get; init; }
    public DateTime Timestamp { get; set; }
    public bool IsCurrent { get; set; }

    public override string ToString()
    {
        return Id;
    }
}