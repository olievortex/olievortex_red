using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OlievortexRed.Lib;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;

namespace OlievortexRed.Functions;

public class TimerDailyStormDownload(
    ILogger<TimerDailyStormDownload> logger,
    ISpcProcess spcProcess,
    ISpcBusiness spcBusiness,
    IRadarBusiness radarBusiness,
    IRadarSource radarSource,
    IDailySummaryBusiness stormyBusiness,
    ISatelliteProcess satelliteProcess,
    ISatelliteSource satelliteSource,
    IMesoProductProcess mesoProductProcess,
    IConfiguration configuration)
{
    private const bool RunOnStartup = false;

    [Function(nameof(TimerDailyStormDownload))]
    public async Task Run([TimerTrigger("0 0 7 * * *", RunOnStartup = RunOnStartup)] TimerInfo myTimer,
        CancellationToken ct)
    {
        if (Program.IsShortCircuit) return;

        try
        {
            logger.LogInformation("OlievortexRed TimerDailyStormDownload triggered");

            await RunImportStormEventsSpcProcess(ct);
            await RunSatelliteAwsInventoryProcess(ct);
            await RunSpcMesosProcess(ct);

            if (myTimer.ScheduleStatus is not null)
                logger.LogInformation("OlievortexRed TimerDailyStormDownload Next timer schedule at: {a}",
                    myTimer.ScheduleStatus.Next);
        }
        catch (Exception ex)
        {
            logger.LogError("OlievortexRed TimerDailyStormDownload Error: {a}", ex.ToString());
            throw;
        }
    }

    private async Task RunImportStormEventsSpcProcess(CancellationToken ct)
    {
        var awsClient = new AmazonS3Client(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
        var process = new ImportStormEventsSpcProcess(spcProcess, spcBusiness, radarBusiness, radarSource);
        await process.RunAsync(awsClient, ct);
    }

    private async Task RunSatelliteAwsInventoryProcess(CancellationToken ct)
    {
        using var client = new AmazonS3Client(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
        var process = new SatelliteInventoryProcess(stormyBusiness, satelliteProcess, satelliteSource);
        await process.RunAsync(client, ct);
    }

    private async Task RunSpcMesosProcess(CancellationToken ct)
    {
        var olieConfig = new OlieConfig(configuration);
        var credOptions = new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true
        };

        var blobClient = new BlobContainerClient(new Uri(olieConfig.OlieBlobGoldContainerUri),
            new DefaultAzureCredential(credOptions));
        var process = new SpcMesosProcess(mesoProductProcess);
        await process.RunAsync(blobClient, true, false, ct);
    }
}