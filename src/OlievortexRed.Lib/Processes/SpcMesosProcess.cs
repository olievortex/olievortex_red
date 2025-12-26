using Azure.Storage.Blobs;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;

namespace OlievortexRed.Lib.Processes;

public class SpcMesosProcess(IMesoProductProcess process)
{
    public async Task RunAsync(BlobContainerClient goldClient, bool isCurrentYearOnly, bool isUpdateOnly,
        CancellationToken ct)
    {
        foreach (var year in CommonProcess.Years)
        {
            if (isCurrentYearOnly && DateTime.UtcNow.Year != year) continue;

            var start = isUpdateOnly ? 0 : await process.GetCurrentMdIndexAsync(year, ct);

            for (var index = start + 1; index < 5000; index++)
                if (!await DoSomethingAsync(year, index, isUpdateOnly, goldClient, ct))
                    break;
        }
    }

    public async Task<bool> DoSomethingAsync(int year, int index, bool isUpdateOnly, BlobContainerClient blobClient,
        CancellationToken ct)
    {
        return isUpdateOnly
            ? await process.UpdateAsync(year, index, ct)
            : await process.DownloadAsync(year, index, blobClient, ct);
    }
}