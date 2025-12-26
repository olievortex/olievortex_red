using System.Net;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;

namespace OlievortexRed.Lib.StormPredictionCenter.Mesos;

public class MesoProductSource(IOlieWebServices ows, ICosmosRepository cosmos) : IMesoProductSource
{
    private const string BaseUrl = "https://www.spc.noaa.gov/products/md/";

    public async Task<string?> DownloadHtmlAsync(int year, int index, CancellationToken ct)
    {
        var url = $"{BaseUrl}{year}/md{index:0000}.html";

        var (response, _, html) = await ows.ApiGetAsync(url, null, ct);
        if (response == HttpStatusCode.NotFound) return null;
        if (response != HttpStatusCode.OK || html is null)
            throw new ApplicationException($"Could not download for {year}, {index}");

        return html;
    }

    public async Task DownloadImageAsync(string imageName, SpcMesoProductEntity product, BlobContainerClient blobClient,
        CancellationToken ct)
    {
        if (product.GraphicUrl is not null) return;

        var dt = product.EffectiveTime;
        var ext = Path.GetExtension(imageName);
        var url = $"{BaseUrl}{dt.Year}/{imageName}";
        var blobFileName = $"gold/spc/meso/{dt.Year}/{dt.Month}/{imageName}";
        var local = CommonProcess.CreateLocalTmpPath(ext);

        var image = await ows.ApiGetBytesAsync(url, ct);
        await ows.FileWriteAllBytesAsync(local, image, ct);
        await ows.BlobUploadFileAsync(blobClient, blobFileName, local, ct);
        ows.FileDelete(local);

        product.GraphicUrl = blobFileName;
        product.Timestamp = DateTime.UtcNow;
        await cosmos.SpcMesoProductUpdateAsync(product, ct);
    }

    public async Task AddToCosmosAsync(SpcMesoProductEntity entity, CancellationToken ct)
    {
        await cosmos.SpcMesoProductCreateAsync(entity, ct);
    }

    public async Task<SpcMesoProductEntity?> GetFromCosmosAsync(int year, int index, CancellationToken ct)
    {
        return await cosmos.SpcMesoProductGetAsync(year, index, ct);
    }

    public async Task UpdateCosmosAsync(SpcMesoProductEntity existing, string areasAffected, string concerning,
        CancellationToken ct)
    {
        if (existing.AreasAffected == areasAffected && existing.Concerning == concerning) return;

        existing.AreasAffected = areasAffected;
        existing.Concerning = concerning;
        existing.Timestamp = DateTime.UtcNow;

        await cosmos.SpcMesoProductUpdateAsync(existing, ct);
    }

    public async Task<int> GetLatestIdForYearAsync(int year, CancellationToken ct)
    {
        var latest = await cosmos.SpcMesoProductGetLatestAsync(year, ct);

        return latest?.Id ?? 0;
    }
}