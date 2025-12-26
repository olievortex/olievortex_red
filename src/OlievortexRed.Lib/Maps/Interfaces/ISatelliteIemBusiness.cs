using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using OlievortexRed.Lib.Maps.Models;

namespace OlievortexRed.Lib.Maps.Interfaces;

public interface ISatelliteIemBusiness
{
    Task DownloadAsync(SatelliteAwsProductEntity product, Func<int, Task> delayFunc,
        BlobContainerClient blobClient, CancellationToken ct);

    Task<AwsKeysModel?> ListKeysAsync(string dayValue, int channel, DayPartsEnum dayPart, CancellationToken ct);
}