using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using SixLabors.ImageSharp;

namespace OlievortexRed.Lib.Maps.Interfaces;

public interface ISatelliteSource
{
    Task AddInventoryToCosmosAsync(string effectiveDate, string bucket, int channel, DayPartsEnum dayPart,
        CancellationToken ct);

    Task AddProductsToCosmosAsync(string[] keys, string effectiveDate, string bucket, int channel, DayPartsEnum dayPart,
        Func<string, DateTime> getScanTimeFunc, CancellationToken ct);

    DateTime GetEffectiveDate(string value);

    DateTime GetEffectiveStart(DateTime effectiveDate, DayPartsEnum dayPart);

    DateTime GetEffectiveStop(DateTime effectiveDate, DayPartsEnum dayPart);

    Task<List<SatelliteAwsInventoryEntity>> GetInventoryByYearAsync(int year, int channel, DayPartsEnum dayPart,
        CancellationToken ct);

    string GetPath(DateTime effectiveDate, string metal);

    Task<SatelliteAwsProductEntity?> GetProductPosterAsync(string effectiveDate, DateTime eventTime,
        CancellationToken ct);

    Task<List<SatelliteAwsProductEntity>> GetProductListAsync(string effectiveDate, string bucketName,
        int channel, CancellationToken ct);

    Task<List<SatelliteAwsProductEntity>> GetProductListNoPosterAsync(CancellationToken ct);

    Task MakePosterAsync(SatelliteAwsProductEntity satellite, Point finalSize, BlobContainerClient goldClient,
        CancellationToken ct);

    Task<bool> MessagePurpleAsync(SatelliteAwsProductEntity satellite, ServiceBusSender sender, CancellationToken ct);

    Task Start1080ContainersAsync(DefaultAzureCredential credential, int instanceLimit, CancellationToken ct);
}