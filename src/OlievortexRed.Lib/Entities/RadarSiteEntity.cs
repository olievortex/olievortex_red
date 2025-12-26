using System.ComponentModel.DataAnnotations;

namespace OlievortexRed.Lib.Entities;

public class RadarSiteEntity
{
    [MaxLength(16)] public string Id { get; init; } = string.Empty;

    [MaxLength(32)] public string Name { get; init; } = string.Empty;
    [MaxLength(8)] public string State { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public DateTime Timestamp { get; init; }
}