namespace OlievortexRed.Cli;

public class OlieArgs
{
    public CommandsEnum Command { get; private set; }
    public string? Restart { get; private set; }

    public enum CommandsEnum
    {
        EventsDatabase,
        LoadRadar,
        MesoHistory
    }

    public OlieArgs(string[] args)
    {
        ParseCommand(args);
        ParseOptions(args);
    }

    private void ParseOptions(string[] args)
    {
        if (args.Length == 1) return;

        for (var i = 1; i < args.Length; i++)
        {
            var parameter = args[i].ToLower();

            switch (parameter)
            {
                case "--restart":
                    Restart = GetParameterString(ref i, args);
                    break;
                default:
                    throw new ArgumentException($"Unknown parameter {parameter}");
            }
        }
    }

    private static string GetParameterString(ref int index, string[] args)
    {
        index++;
        if (index >= args.Length) throw new ArgumentException("Parameter value was missing");

        return args[index];
    }

    private void ParseCommand(string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("The command name is missing");

        var command = args[0].ToLower();

        Command = command switch
        {
            "eventsdatabase" => CommandsEnum.EventsDatabase,
            "loadradar" => CommandsEnum.LoadRadar,
            "mesohistory" => CommandsEnum.MesoHistory,
            _ => throw new ArgumentException($"Unknown command {command}")
        };
    }
}