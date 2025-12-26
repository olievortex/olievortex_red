using System.ComponentModel.DataAnnotations;

namespace OlievortexRed.Lib.Entities;

public class SpcMesoProductEntity
{
    public int Id { get; init; }
    [MaxLength(32)] public string EffectiveDate { get; init; } = string.Empty;

    public DateTime EffectiveTime { get; init; }
    [MaxLength(300)] public string AreasAffected { get; set; } = string.Empty;
    [MaxLength(300)] public string Concerning { get; set; } = string.Empty;
    [MaxLength(300)] public string? GraphicUrl { get; set; }
    [MaxLength(8000)] public string Narrative { get; init; } = string.Empty;
    [MaxLength(8000)] public string Html { get; init; } = string.Empty;
    public DateTime Timestamp { get; set; }
}