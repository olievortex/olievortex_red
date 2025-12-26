using Azure.Storage.Blobs;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormPredictionCenter.Mesos;

namespace OlievortexRed.Cli;

public class CommandMesoHistoryDownload
{
    public async Task Run()
    {
        Console.WriteLine("Download History SPC Mesoscale Discussions");

        // Service
        var di = new OlieCliDi();

        // Base
        var config = di.GetOlieConfig();
        var credential = di.GetDefaultAzureCredential();
        var ows = new OlieWebServices();
        var cosmos = di.GetOlieCosmosRepository();

        // Process
        var mesoSource = new MesoProductSource(ows, cosmos);
        var mesoParse = new MesoProductParsing();
        var mesoProcess = new MesoProductProcess(mesoSource, mesoParse);
        var process = new SpcMesosProcess(mesoProcess);

        // Run
        var ct = CancellationToken.None;
        var blobClient = new BlobContainerClient(new Uri(config.OlieBlobGoldContainerUri), credential);
        await process.RunAsync(blobClient, false, false, ct);
    }
}