using System.Diagnostics;
using Azure.Storage.Blobs;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Maps.Models;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Lib.Maps.Satellite;

public class SatelliteIemBusiness(
    ISatelliteSource source,
    ISatelliteIemSource iemSource,
    IOlieWebServices ows,
    ICosmosRepository cosmos) : ISatelliteIemBusiness
{
    public async Task DownloadAsync(SatelliteAwsProductEntity product, Func<int, Task> delayFunc,
        BlobContainerClient blobClient, CancellationToken ct)
    {
        if (product.PathSource is not null) return;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var effectiveDate = source.GetEffectiveDate(product.EffectiveDate);
        var filename = Path.GetFileName(product.Id);
        var blobName = $"{source.GetPath(effectiveDate, "bronze")}/{filename}";
        var localFilename = CommonProcess.CreateLocalTmpPath(".tif");
        var key = $"{iemSource.GetPrefix(product.ScanTime)}{filename}";
        var attempt = 1;

        while (true)
            try
            {
                var data = await ows.ApiGetBytesAsync(key, ct);
                await ows.FileWriteAllBytesAsync(localFilename, data, ct);
                break;
            }
            catch (Exception)
            {
                if (attempt >= 3) throw;

                await delayFunc(attempt++);
            }

        await ows.BlobUploadFileAsync(blobClient, blobName, localFilename, ct);
        ows.FileDelete(localFilename);

        product.PathSource = blobName;
        product.TimeTakenDownload = (int)stopwatch.Elapsed.TotalSeconds;
        product.Timestamp = DateTime.UtcNow;
        await cosmos.SatelliteAwsProductUpdateAsync(product, ct);
    }

    public async Task<AwsKeysModel?> ListKeysAsync(string dayValue, int channel, DayPartsEnum dayPart,
        CancellationToken ct)
    {
        const string bucket = "IEM";

        var keys = new List<string>();

        var effectiveDate = source.GetEffectiveDate(dayValue);
        if (effectiveDate < new DateTime(2010, 1, 1)) return null;

        var start = source.GetEffectiveStart(effectiveDate, dayPart);
        var stop = source.GetEffectiveStop(effectiveDate, dayPart);
        start = start.AddMinutes(-start.Minute);
        stop = stop.AddMinutes(-stop.Minute).AddHours(1);

        var url = iemSource.GetPrefix(effectiveDate);
        var listFiles = await iemSource.IemListAsync(url, ct);

        keys.AddRange(listFiles
            .Where(item =>
                channel == iemSource.GetChannelFromKey(item) &&
                iemSource.GetScanTimeFromKey(effectiveDate, item) >= start &&
                iemSource.GetScanTimeFromKey(effectiveDate, item) < stop));

        return new AwsKeysModel
        {
            Bucket = bucket,
            Keys = keys.OrderBy(o => o).ToArray(),
            GetScanTimeFunc = v => iemSource.GetScanTimeFromKey(effectiveDate, v)
        };
    }
}