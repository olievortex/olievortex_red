using System.ComponentModel.DataAnnotations;

namespace OlievortexRed.Lib.Entities;

public class StormEventsDailyDetailEntity
{
    [MaxLength(36)] public string Id { get; init; } = string.Empty;
    [MaxLength(32)] public string DateFk { get; init; } = string.Empty;
    [MaxLength(32)] public string SourceFk { get; init; } = string.Empty;

    public DateTime EffectiveTime { get; init; }
    [MaxLength(50)] public string State { get; init; } = string.Empty;
    [MaxLength(50)] public string County { get; init; } = string.Empty;
    [MaxLength(50)] public string City { get; init; } = string.Empty;
    [MaxLength(25)] public string EventType { get; init; } = string.Empty;
    [MaxLength(5)] public string ForecastOffice { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    [MaxLength(5)] public string Magnitude { get; init; } = string.Empty;
    public float Latitude { get; init; }
    public float Longitude { get; init; }
    [MaxLength(1000)] public string Narrative { get; init; } = string.Empty;
    [MaxLength(16)] public string ClosestRadar { get; init; } = string.Empty;
}