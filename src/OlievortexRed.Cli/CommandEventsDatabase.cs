using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Radar;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents;

namespace OlievortexRed.Cli;

public class CommandEventsDatabase
{
    public async Task RunAsync()
    {
        Console.WriteLine("Process Database Events");

        // Input
        var year = InputYear();
        var sourceFk = InputSourceFk();
        if (!year.HasValue || string.IsNullOrWhiteSpace(sourceFk)) return;
        if (!Confirm(year, sourceFk)) return;

        // Run
        do
        {
            if (!await DoIteration(year.Value, sourceFk)) break;
        } while (true);
    }

    private static async Task<bool> DoIteration(int year, string sourceFk)
    {
        Console.WriteLine("Starting a new context");

        // Service
        var di = new OlieCliDi();

        // Base
        var awsClient = new AmazonS3Client(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
        var config = di.GetOlieConfig();
        var credential = di.GetDefaultAzureCredential();
        var ows = new OlieWebServices();
        var cosmos = di.GetOlieCosmosRepository();

        // Process
        var database = new DatabaseBusiness(ows, cosmos);
        var databaseProcess = new DatabaseProcess(database);
        var databaseBusiness = new DatabaseBusiness(ows, cosmos);
        var radarSource = new RadarSource(cosmos, ows);
        var radarBusiness = new RadarBusiness(radarSource);
        var process =
            new ImportStormEventsDatabaseProcess(databaseProcess, databaseBusiness, radarSource, radarBusiness);

        // Run
        var ct = CancellationToken.None;
        var blobClient = new BlobContainerClient(new Uri(config.OlieBlobBronzeContainerUri), credential);
        return await process.RunAsync(year, sourceFk, blobClient, awsClient, ct);
    }

    private static int? InputYear()
    {
        Console.Write("Year: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) return null;
        var year = int.Parse(input);

        return year;
    }

    private static string? InputSourceFk()
    {
        Console.Write("SourceFk: ");
        return Console.ReadLine();
    }

    private static bool Confirm(int? year, string? sourceFk)
    {
        Console.Write($"Import Event Database for Year:{year} with sourceFk:{sourceFk}? ");
        var input = Console.ReadLine();
        return !(string.IsNullOrWhiteSpace(input) || !input.Equals("import"));
    }
}