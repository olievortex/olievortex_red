using System.Globalization;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Mapping;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.StormEvents;

public partial class DatabaseBusiness(IOlieWebServices ows, ICosmosRepository cosmos) :
    IDatabaseBusiness
{
    private const string StormEventsUrl = "https://www.ncei.noaa.gov/pub/data/swdi/stormevents/csvfiles/";

    #region private class StormEventRow

    // ReSharper disable once ClassNeverInstantiated.Local
    private class StormEventRow
    {
        [Name("STATE")] public string State { get; init; } = string.Empty;
        [Name("CZ_NAME")] public string County { get; init; } = string.Empty;
        [Name("BEGIN_LOCATION")] public string City { get; init; } = string.Empty;
        [Name("EVENT_TYPE")] public string EventType { get; init; } = string.Empty;
        [Name("WFO")] public string ForecastOffice { get; init; } = string.Empty;
        [Name("BEGIN_DATE_TIME")] public string Effective { get; init; } = string.Empty;
        [Name("CZ_TIMEZONE")] public string TimeZone { get; init; } = string.Empty;

        [Name("MAGNITUDE")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public double? Magnitude { get; init; }

        [Name("TOR_F_SCALE")] public string TornadoFScale { get; init; } = string.Empty;

        [Name("BEGIN_LAT")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public float? Latitude { get; init; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        [Name("BEGIN_LON")] public float? Longitude { get; init; }
        [Name("EVENT_NARRATIVE")] public string Narrative { get; init; } = string.Empty;
    }

    #endregion

    #region Detail

    public async Task AddDailyDetailToCosmosAsync(List<DailyDetailModel> models, string sourceFk, CancellationToken ct)
    {
        var entities = EntityMapping.ToStormEventsDailyDetail(models, sourceFk);
        await cosmos.StormEventsDailyDetailCreateAsync(entities, ct);
    }

    public async Task CompareDetailCountAsync(string dateFk, string sourceFk, int count, CancellationToken ct)
    {
        var actual = await cosmos.StormEventsDailyDetailCountAsync(dateFk, sourceFk, ct);

        if (actual != count)
            throw new InvalidOperationException($"dateFk: {dateFk}, sourceFk: {sourceFk}, count: {count}");
    }

    public async Task DeleteDetailAsync(string dateFk, string sourceFk, CancellationToken ct)
    {
        await cosmos.StormEventsDailyDetailDeleteAsync(dateFk, sourceFk, ct);
    }

    #endregion

    #region Summary

    public async Task ActivateSummaryAsync(StormEventsDailySummaryEntity entity, CancellationToken ct)
    {
        entity.IsCurrent = true;
        entity.Timestamp = DateTime.UtcNow;

        await cosmos.StormEventsDailySummaryUpdateAsync(entity, ct);
    }

    public async Task AddDailySummaryToCosmosAsync(DailySummaryModel model, string sourceFk, CancellationToken ct)
    {
        var entity = EntityMapping.ToStormEventsDailySummary([model], sourceFk)[0];

        await cosmos.StormEventsDailySummaryCreateAsync(entity, ct);
    }

    public async Task DeactivateSummaryAsync(StormEventsDailySummaryEntity entity, CancellationToken ct)
    {
        entity.IsCurrent = false;
        entity.Timestamp = DateTime.UtcNow;

        await cosmos.StormEventsDailySummaryUpdateAsync(entity, ct);
    }

    public async Task<List<StormEventsDailySummaryEntity>> GetSummariesForDayAsync(string id, int year,
        CancellationToken ct)
    {
        return await cosmos.StormEventsDailySummaryListSummariesForDate(id, year, ct);
    }

    #endregion

    #region Database

    public async Task DatabaseDownloadAsync(BlobContainerClient client, List<DatabaseFileModel> model,
        CancellationToken ct)
    {
        var inventory = await cosmos.StormEventsDatabaseInventoryAllAsync(ct);

        foreach (var csv in model)
        {
            if (csv.Year < 2010) continue;

            var entity = inventory
                .SingleOrDefault(w => w.Id == csv.Updated && w.Year == csv.Year);
            if (entity is not null) continue;

            var localFileName = CommonProcess.CreateLocalTmpPath(".csv.gz");
            var fileName = $"bronze/storm-events/{csv.Name}";
            var content = await ows.ApiGetBytesAsync($"{StormEventsUrl}/{csv.Name}", ct);
            await ows.FileWriteAllBytesAsync(localFileName, content, ct);
            await ows.BlobUploadFileAsync(client, fileName, localFileName, ct);

            entity = new StormEventsDatabaseInventoryEntity
            {
                Id = csv.Updated,
                Year = csv.Year,
                BlobName = fileName,
                Timestamp = DateTime.UtcNow
            };

            await cosmos.StormEventsDatabaseInventoryCreateAsync(entity, ct);
            CommonProcess.DeleteTempFiles([localFileName], ows);
        }
    }

    public async Task<StormEventsDatabaseInventoryEntity?> DatabaseGetInventoryAsync(int year, string id,
        CancellationToken ct)
    {
        return await cosmos.StormEventsDatabaseInventoryGetAsync(year, id, ct);
    }

    public async Task<List<DailyDetailModel>> DatabaseLoadAsync(
        BlobContainerClient blobClient, StormEventsDatabaseInventoryEntity eventsDatabase, CancellationToken ct)
    {
        var localFileName = CommonProcess.CreateLocalTmpPath(".csv.gz");
        await ows.BlobDownloadFileAsync(blobClient, eventsDatabase.BlobName, localFileName, ct);
        var csv = await ows.FileReadAllTextFromGzAsync(localFileName, ct);
        using var sr = new StringReader(csv);

        var result = DatabaseParse(sr);

        CommonProcess.DeleteTempFiles([localFileName], ows);

        return result;
    }

    public static List<DailyDetailModel> DatabaseParse(TextReader textReader)
    {
        var result = new List<DailyDetailModel>();
        using var reader = new CsvReader(textReader, CultureInfo.InvariantCulture, true);
        var records = reader.GetRecords<StormEventRow>();

        foreach (var record in records)
        {
            string magnitude;

            switch (record.EventType)
            {
                case "Hail":
                    magnitude = $"{record.Magnitude:0.00}";
                    break;
                case "Thunderstorm Wind":
                    magnitude = $"{record.Magnitude}";
                    break;
                case "Tornado":
                    magnitude = record.TornadoFScale;
                    break;
                default:
                    continue;
            }

            var offset = int.Parse(StripTimeZoneRegex().Replace(record.TimeZone, ""));
            var effective = DateTime
                .ParseExact(record.Effective, "dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)
                .AddHours(-offset);

            var model = new DailyDetailModel
            {
                City = record.City,
                County = record.County,
                Effective = effective,
                EventType = record.EventType,
                ForecastOffice = record.ForecastOffice,
                Latitude = record.Latitude ?? float.NaN,
                Longitude = record.Longitude ?? float.NaN,
                Magnitude = magnitude,
                Narrative = record.Narrative,
                State = record.State
            };
            result.Add(model);
        }

        return result;
    }

    public async Task DatabaseUpdateActiveAsync(StormEventsDatabaseInventoryEntity entity, CancellationToken ct)
    {
        entity.IsActive = true;
        entity.Timestamp = DateTime.UtcNow;

        await cosmos.StormEventsDatabaseInventoryUpdateAsync(entity, ct);
    }

    public async Task DatabaseUpdateRowCountAsync(StormEventsDatabaseInventoryEntity entity, int rowCount,
        CancellationToken ct)
    {
        entity.RowCount = rowCount;
        entity.Timestamp = DateTime.UtcNow;

        await cosmos.StormEventsDatabaseInventoryUpdateAsync(entity, ct);
    }

    public async Task<List<DatabaseFileModel>> DatabaseListAsync(CancellationToken ct)
    {
        var results = await ows.ApiGetStringAsync(StormEventsUrl, ct);
        var matches = MatchCsvFileRegex().Matches(results);

        return matches.Select(s => new DatabaseFileModel
        {
            Name = s.Value
        }).ToList();
    }

    #endregion

    #region Regex

    [GeneratedRegex("[^0-9-]+")]
    private static partial Regex StripTimeZoneRegex();

    [GeneratedRegex(@"StormEvents_details-ftp_v1\.0_d\d{4}_c\d{8}\.csv\.gz(?=\"")")]
    private static partial Regex MatchCsvFileRegex();

    #endregion
}