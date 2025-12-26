using Azure.Storage.Blobs;

namespace OlievortexRed.Lib.StormPredictionCenter.Interfaces;

public interface IMesoProductProcess
{
    Task<bool> DownloadAsync(int year, int index, BlobContainerClient blobClient, CancellationToken ct);
    Task<int> GetCurrentMdIndexAsync(int year, CancellationToken ct);
    Task<bool> UpdateAsync(int year, int index, CancellationToken ct);
}