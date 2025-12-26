namespace OlievortexRed.Lib.Maps.Models;

public class AwsKeysModel
{
    public string Bucket { get; init; } = string.Empty;
    public string[] Keys { get; init; } = [];
    public required Func<string, DateTime> GetScanTimeFunc { get; init; }
}