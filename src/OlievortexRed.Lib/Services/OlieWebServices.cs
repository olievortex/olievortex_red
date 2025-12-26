using Amazon.S3;
using Amazon.S3.Model;
using Azure;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.ResourceManager;
using Azure.ResourceManager.ContainerInstance;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;

namespace OlievortexRed.Lib.Services;

[ExcludeFromCodeCoverage]
public class OlieWebServices : IOlieWebServices
{
    #region Containers

    public async Task<List<string>> StartContainerGroupsAsync(ContainerStartInfo info, int limit, CancellationToken ct)
    {
        var result = new List<string>();
        var client = new ArmClient(info.Credential);
        var subscription = client.GetSubscriptionResource(new ResourceIdentifier(info.SubscriptionId));
        var resourceGroup =
            await subscription.GetResourceGroupAsync(info.ResourceGroupName, ct);
        var containerGroups = resourceGroup.Value.GetContainerGroups();
        var startCount = 0;

        foreach (var containerGroup in containerGroups)
        {
            if (!containerGroup.Id.Name.StartsWith(info.ContainerGroupNameStart)) continue;

            result.Add(containerGroup.Id.Name);
            await containerGroup.StartAsync(WaitUntil.Started, ct);

            startCount++;

            if (startCount >= limit) break;
        }

        return result;
    }

    #endregion

    #region Api

    public async Task<byte[]> ApiGetBytesAsync(string url, CancellationToken ct)
    {
        using var hc = new HttpClient();
        var body = await hc.GetByteArrayAsync(url, ct);

        return body;
    }

    public async Task<string> ApiGetStringAsync(string url, CancellationToken ct)
    {
        using var hc = new HttpClient();
        var body = await hc.GetStringAsync(url, ct);

        return body;
    }

    public async Task<(HttpStatusCode, EntityTagHeaderValue?, string)> ApiGetAsync(string url,
        EntityTagHeaderValue? etag, CancellationToken ct)
    {
        using var hc = new HttpClient();
        hc.Timeout = TimeSpan.FromSeconds(30);

        if (etag is not null) hc.DefaultRequestHeaders.IfNoneMatch.Add(etag);

        try
        {
            using var response = await hc.GetAsync(url, ct);

            var body = await response.Content.ReadAsStringAsync(ct);
            var etagResponse = response.Headers.ETag;
            var responseCode = response.StatusCode;

            return (responseCode, etagResponse, body);
        }
        catch (OperationCanceledException)
        {
            return (HttpStatusCode.NotFound, null, string.Empty);
        }
    }

    #endregion

    #region Aws

    public async Task AwsDownloadAsync(string filename, string bucketName, string key, IAmazonS3 client,
        CancellationToken ct)
    {
        var response = await client.GetObjectAsync(bucketName, key, ct);
        await response.WriteResponseStreamToFileAsync(filename, false, ct);
    }

    public async Task<List<string>> AwsListAsync(string bucketName, string prefix, IAmazonS3 client,
        CancellationToken ct)
    {
        var result = new List<string>();
        ListObjectsV2Response response;

        var request = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = prefix
        };

        do
        {
            response = await client.ListObjectsV2Async(request, ct);

            result.AddRange(response.S3Objects.Select(s => s.Key));

            // If the response is truncated, set the request ContinuationToken
            // from the NextContinuationToken property of the response.
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated ?? false);

        return result;
    }

    #endregion

    #region Blob

    public async Task BlobDownloadFileAsync(BlobContainerClient client, string fileName, string localFileName,
        CancellationToken ct)
    {
        var blobClient = client.GetBlobClient(fileName);
        await blobClient.DownloadToAsync(localFileName, ct);
    }

    public async Task BlobUploadFileAsync(BlobContainerClient client, string fileName, string localFileName,
        CancellationToken ct)
    {
        var blobClient = client.GetBlobClient(fileName);
        var contentType = "application/octet-stream";
        var extension = Path.GetExtension(fileName);

        if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)) contentType = "image/gif";

        if (extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)) contentType = "video/mp4";

        var headers = new BlobHttpHeaders
        {
            CacheControl = "public, max-age=604800",
            ContentType = contentType
        };

        await blobClient.UploadAsync(localFileName, headers, cancellationToken: ct);
    }

    #endregion

    #region File

    public void FileDelete(string path)
    {
        File.Delete(path);
    }

    public async Task<byte[]> FileReadAllBytes(string path, CancellationToken ct)
    {
        return await File.ReadAllBytesAsync(path, ct);
    }

    public async Task<string> FileReadAllTextFromGzAsync(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        await using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        using var sr = new StreamReader(gzip);
        var result = await sr.ReadToEndAsync(ct);

        return result;
    }

    public async Task FileWriteAllBytesAsync(string path, byte[] data, CancellationToken ct)
    {
        await File.WriteAllBytesAsync(path, data, ct);
    }

    #endregion

    #region ServiceBus

    public async Task ServiceBusSendJsonAsync(ServiceBusSender sender, object data, CancellationToken ct)
    {
        var json = JsonConvert.SerializeObject(data);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(message, ct);
    }

    #endregion
}