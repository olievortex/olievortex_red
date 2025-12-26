namespace OlievortexRed.Lib.StormEvents.Models;

public class DailyDetailModel
{
    public string EffectiveDate => $"{Effective.AddHours(-12):yyyy-MM-dd}";
    public string State { get; init; } = string.Empty;
    public string County { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string ForecastOffice { get; init; } = string.Empty;
    public DateTime Effective { get; init; }
    public string Magnitude { get; init; } = string.Empty;
    public float Latitude { get; init; }
    public float Longitude { get; init; }
    public string Narrative { get; init; } = string.Empty;
    public string ClosestRadar { get; set; } = string.Empty;
}