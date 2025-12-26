using System.ComponentModel.DataAnnotations;
using OlievortexRed.Lib.Enums;

namespace OlievortexRed.Lib.Entities;

public class SatelliteAwsProductEntity
{
    [MaxLength(100)] public string Id { get; init; } = string.Empty; // AWS Key
    [MaxLength(32)] public string EffectiveDate { get; init; } = string.Empty; // Partition Key cannot be DateTime

    [MaxLength(50)] public string BucketName { get; init; } = string.Empty;
    public int Channel { get; init; } // 2 = Red Visible (HiRes)
    public DayPartsEnum DayPart { get; init; } // 3 = Afternoon
    [MaxLength(320)] public string? Path1080 { get; init; }
    [MaxLength(320)] public string? PathPoster { get; set; }
    [MaxLength(320)] public string? PathSource { get; set; }
    public DateTime ScanTime { get; init; }
    public DateTime Timestamp { get; set; }
    public int TimeTaken1080 { get; init; }
    public int TimeTakenDownload { get; set; }
    public int TimeTakenPoster { get; set; }
}