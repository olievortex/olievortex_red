namespace OlievortexRed.Lib.StormEvents.Models;

public class DailySummaryModel
{
    public string EffectiveDate { get; init; } = string.Empty;
    public int Hail { get; init; }
    public int Wind { get; init; }
    public int F5 { get; init; }
    public int F4 { get; init; }
    public int F3 { get; init; }
    public int F2 { get; init; }
    public int F1 { get; init; }
    public DateTime? HeadlineEventTime { get; init; }
}