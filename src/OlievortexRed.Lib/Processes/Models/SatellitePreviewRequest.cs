namespace OlievortexRed.Lib.Processes.Models;

public class SatellitePreviewRequest
{
    public string EffectiveDate { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    public int Channel { get; init; }
    public int Year { get; init; }
    public List<string> Keys { get; init; } = [];
}