namespace OlievortexRed.Lib.StormEvents.Models;

public class DatabaseFileModel
{
    public string Name { get; init; } = string.Empty;

    public int Year
    {
        get
        {
            var part = Name[30..34];
            return int.Parse(part);
        }
    }

    public string Updated
    {
        get
        {
            var part = Name[36..44];
            return part;
        }
    }
}