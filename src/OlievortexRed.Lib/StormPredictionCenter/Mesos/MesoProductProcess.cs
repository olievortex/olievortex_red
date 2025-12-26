using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;

namespace OlievortexRed.Lib.StormPredictionCenter.Mesos;

public class MesoProductProcess(IMesoProductSource source, IMesoProductParsing parse) : IMesoProductProcess
{
    public async Task<bool> DownloadAsync(int year, int index, BlobContainerClient blobClient, CancellationToken ct)
    {
        var html = await source.DownloadHtmlAsync(year, index, ct);
        if (html is null) return false;

        var body = parse.GetBody(html);
        var effectiveTime = parse.GetEffectiveTime(body);

        var entity = new SpcMesoProductEntity
        {
            Id = index,
            EffectiveDate = effectiveTime.AddHours(-12).ToString("yyyy-MM-dd"),

            AreasAffected = parse.GetAreasAffected(body),
            Concerning = parse.GetConcerning(body),
            EffectiveTime = effectiveTime,
            Narrative = parse.GetNarrative(body),
            Html = html,
            Timestamp = DateTime.UtcNow
        };

        await source.AddToCosmosAsync(entity, ct);
        await source.DownloadImageAsync(parse.GetImageName(html), entity, blobClient, ct);

        return true;
    }

    public async Task<int> GetCurrentMdIndexAsync(int year, CancellationToken ct)
    {
        return await source.GetLatestIdForYearAsync(year, ct);
    }

    public async Task<bool> UpdateAsync(int year, int index, CancellationToken ct)
    {
        var entity = await source.GetFromCosmosAsync(year, index, ct);
        if (entity is null) return false;

        var body = parse.GetBody(entity.Html);

        var updatedAreasAffected = parse.GetAreasAffected(body);
        var updatedConcerning = parse.GetConcerning(body);

        await source.UpdateCosmosAsync(entity, updatedAreasAffected, updatedConcerning, ct);

        return true;
    }
}