using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;

namespace OlievortexRed.Lib.StormPredictionCenter.Interfaces;

public interface IMesoProductSource
{
    Task AddToCosmosAsync(SpcMesoProductEntity entity, CancellationToken ct);

    Task<string?> DownloadHtmlAsync(int year, int index, CancellationToken ct);

    Task DownloadImageAsync(string imageName, SpcMesoProductEntity product, BlobContainerClient blobClient,
        CancellationToken ct);

    Task<SpcMesoProductEntity?> GetFromCosmosAsync(int year, int index, CancellationToken ct);

    Task<int> GetLatestIdForYearAsync(int year, CancellationToken ct);

    Task UpdateCosmosAsync(SpcMesoProductEntity existing, string areasAffected, string concerning,
        CancellationToken ct);
}