using Amazon.S3;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using SixLabors.ImageSharp;

namespace OlievortexRed.Lib.Maps.Satellite;

public class SatelliteProcess(
    ISatelliteSource source,
    ISatelliteAwsBusiness awsBusiness,
    ISatelliteIemBusiness iemBusiness,
    IDailySummaryBusiness summaryBusiness) : ISatelliteProcess
{
    // GOES 16 became operational during December 2017
    private const int Goes16 = 2018;

    public async Task CreatePosterAsync(SatelliteAwsProductEntity satellite, StormEventsDailySummaryEntity summary,
        Point finalSize, BlobContainerClient goldClient, CancellationToken ct)
    {
        if (summary.SatellitePath1080 is not null && summary.SatellitePathPoster is null)
        {
            if (satellite.PathPoster is null)
                await source.MakePosterAsync(satellite, finalSize, goldClient, ct);

            summary.SatellitePathPoster = satellite.PathPoster;
            await summaryBusiness.UpdateCosmosAsync(summary, ct);
        }
    }

    public async Task<SatelliteAwsProductEntity?> GetSatelliteProductIncompleteAsync(
        StormEventsDailySummaryEntity summary, CancellationToken ct)
    {
        if (summary.HeadlineEventTime is null) return null;
        if (summary.SatellitePathPoster is not null && summary.SatellitePath1080 is not null) return null;

        var satellite = await source.GetProductPosterAsync(summary.Id, summary.HeadlineEventTime.Value,
            ct);

        return satellite;
    }

    public async Task ProcessMissingDayAsync(int year, string missingDay, int satellite, int channel,
        DayPartsEnum dayPart, IAmazonS3 client, CancellationToken ct)
    {
        var result = year < Goes16
            ? await iemBusiness.ListKeysAsync(missingDay, channel, dayPart, ct)
            : await awsBusiness.ListAwsKeysAsync(missingDay, satellite, channel, dayPart, client, ct);
        if (result is null || result.Keys.Length == 0) return;

        await source.AddProductsToCosmosAsync(result.Keys, missingDay, result.Bucket, channel, dayPart,
            result.GetScanTimeFunc, ct);
        await source.AddInventoryToCosmosAsync(missingDay, result.Bucket, channel, dayPart, ct);
    }

    public async Task<bool> Source1080Async(int year, SatelliteAwsProductEntity satellite, Func<int, Task> delayFunc,
        ServiceBusSender sender, BlobContainerClient blobClient, IAmazonS3 awsClient, CancellationToken ct)
    {
        if (year < Goes16)
            await iemBusiness.DownloadAsync(satellite, delayFunc, blobClient, ct);
        else
            await awsBusiness.DownloadAsync(satellite, delayFunc, blobClient, awsClient, ct);

        return await source.MessagePurpleAsync(satellite, sender, ct);
    }

    public async Task Update1080Async(SatelliteAwsProductEntity satellite, StormEventsDailySummaryEntity summary,
        CancellationToken ct)
    {
        if (summary.SatellitePath1080 is null && satellite.Path1080 is not null)
        {
            summary.SatellitePath1080 = satellite.Path1080;
            await summaryBusiness.UpdateCosmosAsync(summary, ct);
        }
    }
}