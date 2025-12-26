using System.ComponentModel.DataAnnotations;

namespace OlievortexRed.Lib.Entities;

public class RadarInventoryEntity
{
    [MaxLength(16)] public string Id { get; init; } = string.Empty; // Radar Id
    [MaxLength(32)] public string EffectiveDate { get; init; } = string.Empty;
    [MaxLength(50)] public string BucketName { get; init; } = string.Empty;

    public List<string> FileList { get; init; } = [];
    public DateTime Timestamp { get; init; }
}