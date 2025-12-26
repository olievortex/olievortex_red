using System.ComponentModel.DataAnnotations;
using OlievortexRed.Lib.Enums;

namespace OlievortexRed.Lib.Entities;

public class SatelliteAwsInventoryEntity
{
    [MaxLength(36)] public string Id { get; init; } = string.Empty; // AWS Bucket
    [MaxLength(32)] public string EffectiveDate { get; init; } = string.Empty; // Partition Key cannot be DateTime
    public int Channel { get; init; }
    public DayPartsEnum DayPart { get; init; }

    public DateTime Timestamp { get; init; }
}