using Amazon.S3;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using SixLabors.ImageSharp;

namespace OlievortexRed.Lib.Processes;

public class SatellitePosterProcess(
    ISatelliteProcess satelliteProcess,
    ISatelliteSource satelliteSource,
    IDailySummaryBusiness summaryBusiness)
{
    private readonly Point _finalSize = new(1246, 540);
    private bool _startContainer;

    public async Task RunAsync(int containerLimit, ServiceBusSender sender, Func<int, Task> delayFunc,
        BlobContainerClient bronzeClient, BlobContainerClient goldClient, IAmazonS3 awsClient,
        DefaultAzureCredential credential, CancellationToken ct)
    {
        _startContainer = false;

        foreach (var year in CommonProcess.Years)
            await AnnualProcessAsync(year, delayFunc, sender, bronzeClient, goldClient, awsClient, ct);

        await RequestProcessAsync(goldClient, ct);

        if (_startContainer) await satelliteSource.Start1080ContainersAsync(credential, containerLimit, ct);
    }

    private async Task RequestProcessAsync(BlobContainerClient goldClient, CancellationToken ct)
    {
        var missingPosters = await satelliteSource.GetProductListNoPosterAsync(ct);

        foreach (var missingPoster in missingPosters)
            await satelliteSource.MakePosterAsync(missingPoster, _finalSize, goldClient, ct);
    }

    public async Task AnnualProcessAsync(int year, Func<int, Task> delayFunc, ServiceBusSender sender,
        BlobContainerClient bronzeClient, BlobContainerClient goldClient, IAmazonS3 awsClient, CancellationToken ct)
    {
        var missingPosters = await summaryBusiness.GetMissingPostersByYearAsync(year, ct);

        foreach (var missingPoster in missingPosters)
        {
            var satellite = await satelliteProcess.GetSatelliteProductIncompleteAsync(missingPoster, ct);
            if (satellite is null) continue;

            _startContainer |=
                await satelliteProcess.Source1080Async(year, satellite, delayFunc, sender, bronzeClient, awsClient, ct);
            await satelliteProcess.Update1080Async(satellite, missingPoster, ct);
            await satelliteProcess.CreatePosterAsync(satellite, missingPoster, _finalSize, goldClient, ct);
        }
    }
}