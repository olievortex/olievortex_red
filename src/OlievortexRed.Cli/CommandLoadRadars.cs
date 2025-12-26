using OlievortexRed.Lib.Radar;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Cli;

public class CommandLoadRadars
{
    public async Task Run()
    {
        var ct = CancellationToken.None;

        var di = new OlieCliDi();
        var ows = new OlieWebServices();
        var source = new RadarSource(di.GetOlieCosmosRepository(), ows);
        var process = new RadarBusiness(source);
        var value = await File.ReadAllTextAsync("./Resources/nexrad-stations.txt", ct);

        await process.PopulateRadarSitesFromCsvAsync(value, ct);
    }
}