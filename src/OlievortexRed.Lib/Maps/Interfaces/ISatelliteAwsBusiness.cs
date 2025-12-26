using Amazon.S3;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using OlievortexRed.Lib.Maps.Models;

namespace OlievortexRed.Lib.Maps.Interfaces;

public interface ISatelliteAwsBusiness
{
    Task DownloadAsync(SatelliteAwsProductEntity product, Func<int, Task> delayFunc, BlobContainerClient blobClient,
        IAmazonS3 awsClient, CancellationToken ct);

    Task<AwsKeysModel?> ListAwsKeysAsync(string dayValue, int satellite, int channel, DayPartsEnum dayPart,
        IAmazonS3 client, CancellationToken ct);
}