using Amazon.S3;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using System.Net;
using System.Net.Http.Headers;

namespace OlievortexRed.Lib.Services;

public interface IOlieWebServices
{
    #region Containers

    Task<List<string>> StartContainerGroupsAsync(ContainerStartInfo info, int limit, CancellationToken ct);

    #endregion

    #region Api

    Task<(HttpStatusCode, EntityTagHeaderValue?, string)> ApiGetAsync(string url, EntityTagHeaderValue? etag,
        CancellationToken ct);

    Task<byte[]> ApiGetBytesAsync(string url, CancellationToken ct);
    Task<string> ApiGetStringAsync(string url, CancellationToken ct);

    #endregion

    #region Aws

    Task AwsDownloadAsync(string filename, string bucketName, string key, IAmazonS3 client, CancellationToken ct);
    Task<List<string>> AwsListAsync(string bucketName, string prefix, IAmazonS3 client, CancellationToken ct);

    #endregion

    #region Blob

    Task BlobDownloadFileAsync(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct);
    Task BlobUploadFileAsync(BlobContainerClient client, string fileName, string localFileName, CancellationToken ct);

    #endregion

    #region File

    void FileDelete(string path);
    Task<byte[]> FileReadAllBytes(string path, CancellationToken ct);
    Task<string> FileReadAllTextFromGzAsync(string path, CancellationToken ct);
    Task FileWriteAllBytesAsync(string path, byte[] data, CancellationToken ct);

    #endregion

    #region ServiceBus

    Task ServiceBusSendJsonAsync(ServiceBusSender sender, object data, CancellationToken ct);

    #endregion
}