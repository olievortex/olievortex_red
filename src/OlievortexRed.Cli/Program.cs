namespace OlievortexRed.Cli;

internal static class Program
{
    private static void Main(string[] args)
    {
        var t = MainAsync(args);
        t.Wait();
    }

    private static async Task MainAsync(string[] args)
    {
        Console.WriteLine($"{DateTime.UtcNow:u} OlieVortexRed.Cli");
        Console.WriteLine();

        var olieArgs = new OlieArgs(args);

        switch (olieArgs.Command)
        {
            case OlieArgs.CommandsEnum.EventsDatabase:
                await new CommandEventsDatabase().RunAsync();
                break;
            case OlieArgs.CommandsEnum.LoadRadar:
                await new CommandLoadRadars().Run();
                break;
            case OlieArgs.CommandsEnum.MesoHistory:
                await new CommandMesoHistoryDownload().Run();
                break;
            default:
                throw new ArgumentException($"The command {olieArgs.Command} is not implemented yet.");
        }

        Console.WriteLine($"{DateTime.UtcNow:u} Clean exit");
    }
}