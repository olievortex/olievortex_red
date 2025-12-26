using Amazon.S3;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using SixLabors.ImageSharp;

namespace OlievortexRed.Lib.Maps.Interfaces;

public interface ISatelliteProcess
{
    Task CreatePosterAsync(SatelliteAwsProductEntity satellite, StormEventsDailySummaryEntity summary,
        Point finalSize, BlobContainerClient goldClient, CancellationToken ct);

    Task<SatelliteAwsProductEntity?> GetSatelliteProductIncompleteAsync(StormEventsDailySummaryEntity summary,
        CancellationToken ct);

    Task ProcessMissingDayAsync(int year, string missingDay, int satellite, int channel,
        DayPartsEnum dayPart, IAmazonS3 client, CancellationToken ct);

    Task<bool> Source1080Async(int year, SatelliteAwsProductEntity satellite, Func<int, Task> delayFunc,
        ServiceBusSender sender, BlobContainerClient blobClient, IAmazonS3 awsClient, CancellationToken ct);

    Task Update1080Async(SatelliteAwsProductEntity satellite, StormEventsDailySummaryEntity summary,
        CancellationToken ct);
}