using Amazon.S3;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes.Models;

namespace OlievortexRed.Lib.Processes;

public class SatellitePreviewProcess(ISatelliteSource satelliteSource, ISatelliteProcess satelliteProcess)
{
    public async Task RunAsync(SatellitePreviewRequest request, IAmazonS3 awsClient,
        int containerLimit, ServiceBusSender sender, Func<int, Task> delayFunc,
        BlobContainerClient blobClient, DefaultAzureCredential credential, CancellationToken ct)
    {
        var startContainer = false;
        var productList = await GetProductListAsync(request, ct);

        foreach (var product in productList)
            startContainer |=
                await satelliteProcess.Source1080Async(request.Year, product, delayFunc, sender, blobClient, awsClient,
                    ct);

        if (startContainer) await satelliteSource.Start1080ContainersAsync(credential, containerLimit, ct);
    }

    private async Task<List<SatelliteAwsProductEntity>> GetProductListAsync(SatellitePreviewRequest request,
        CancellationToken ct)
    {
        var productList = (await satelliteSource.GetProductListAsync(
                request.EffectiveDate, request.BucketName, request.Channel, ct))
            .Where(w => request.Keys.Contains(w.Id))
            .ToList();

        return productList;
    }
}