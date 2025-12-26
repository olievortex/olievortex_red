using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Lib.Mapping;

public static class EntityMapping
{
    public static List<StormEventsDailyDetailEntity> ToStormEventsDailyDetail(List<DailyDetailModel> models,
        string sourceFk)
    {
        return models.Select(s => new StormEventsDailyDetailEntity
        {
            City = s.City,
            County = s.County,
            DateFk = s.EffectiveDate,
            EventType = s.EventType,
            ForecastOffice = s.ForecastOffice,
            Id = Guid.NewGuid().ToString(),
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            Magnitude = s.Magnitude,
            Narrative = s.Narrative,
            State = s.State,
            EffectiveTime = s.Effective,
            Timestamp = DateTime.UtcNow,
            SourceFk = sourceFk,
            ClosestRadar = s.ClosestRadar
        }).ToList();
    }

    public static List<StormEventsDailySummaryEntity> ToStormEventsDailySummary(
        List<DailySummaryModel> models, string sourceFk)
    {
        return models.Select(day => new StormEventsDailySummaryEntity
        {
            Id = day.EffectiveDate,
            Year = int.Parse(day.EffectiveDate[..4]),

            SourceFk = sourceFk,
            Hail = day.Hail,
            Wind = day.Wind,
            F5 = day.F5,
            F4 = day.F4,
            F3 = day.F3,
            F2 = day.F2,
            F1 = day.F1,
            Timestamp = DateTime.UtcNow,
            HeadlineEventTime = day.HeadlineEventTime,
            IsCurrent = false,
            RowCount = day.Hail + day.Wind + day.F5 + day.F4 + day.F3 + day.F2 + day.F1
        }).ToList();
    }
}