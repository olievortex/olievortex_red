namespace OlievortexRed.Lib.StormPredictionCenter.Interfaces;

public interface IMesoProductParsing
{
    string GetAreasAffected(string body);
    string GetBody(string html);
    string GetConcerning(string body);
    DateTime GetEffectiveTime(string body);
    string GetImageName(string html);
    string GetNarrative(string body);
}