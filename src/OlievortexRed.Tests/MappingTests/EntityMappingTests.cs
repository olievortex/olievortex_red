using OlievortexRed.Lib.Mapping;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.MappingTests;

public class EntityMappingTests
{
    #region ToStormEventsDailyDetailEntity

    [Test]
    public void ToStormEventsDailyDetailEntity_Maps_Models()
    {
        // Arrange
        var sourceFk = Guid.NewGuid().ToString();
        var city = Guid.NewGuid().ToString();
        var county = Guid.NewGuid().ToString();
        var effective = new DateTime(2010, 7, 10, 1, 2, 3);
        var eventType = Guid.NewGuid().ToString();
        var state = Guid.NewGuid().ToString();
        var wfo = Guid.NewGuid().ToString();
        var magnitude = Guid.NewGuid().ToString();
        const float lat = 45;
        const float lon = -93;
        var narrative = Guid.NewGuid().ToString();
        var models = new List<DailyDetailModel>
        {
            new()
            {
                City = city,
                County = county,
                Effective = effective,
                EventType = eventType,
                State = state,
                ForecastOffice = wfo,
                Magnitude = magnitude,
                Latitude = lat,
                Longitude = lon,
                Narrative = narrative,
                ClosestRadar = "KMSP"
            }
        };

        // Act
        var results = EntityMapping.ToStormEventsDailyDetail(models, sourceFk);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Magnitude, Is.EqualTo(magnitude));
            Assert.That(results[0].City, Is.EqualTo(city));
            Assert.That(results[0].County, Is.EqualTo(county));
            Assert.That(results[0].EventType, Is.EqualTo(eventType));
            Assert.That(results[0].Id, Is.Not.Null);
            Assert.That(results[0].Latitude, Is.EqualTo(lat));
            Assert.That(results[0].Longitude, Is.EqualTo(lon));
            Assert.That(results[0].Narrative, Is.EqualTo(narrative));
            Assert.That(results[0].ForecastOffice, Is.EqualTo(wfo));
            Assert.That(results[0].DateFk, Is.EqualTo("2010-07-09"));
            Assert.That(results[0].SourceFk, Is.EqualTo(sourceFk));
            Assert.That(results[0].State, Is.EqualTo(state));
            Assert.That(results[0].EffectiveTime, Is.EqualTo(effective));
            Assert.That(results[0].Timestamp, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(results[0].ClosestRadar, Is.EqualTo("KMSP"));
            Assert.That(models[0].EffectiveDate, Is.EqualTo("2010-07-09"));
        });
    }

    #endregion

    #region ToStormEventsDailySummary

    [Test]
    public void ToStormEventsDailySummary_Maps_Models()
    {
        // Arrange
        var sourceFk = Guid.NewGuid().ToString();
        var effective = new DateTime(2010, 7, 10);
        var time = new DateTime(2010, 7, 10, 18, 23, 14);
        var models = new List<DailySummaryModel>
        {
            new()
            {
                EffectiveDate = "2010-07-10",
                F1 = 1,
                F2 = 2,
                F3 = 3,
                F4 = 4,
                F5 = 5,
                Hail = 6,
                Wind = 7,
                HeadlineEventTime = time
            }
        };

        // Act
        var results = EntityMapping.ToStormEventsDailySummary(models, sourceFk);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].F1, Is.EqualTo(1));
            Assert.That(results[0].F2, Is.EqualTo(2));
            Assert.That(results[0].F3, Is.EqualTo(3));
            Assert.That(results[0].F4, Is.EqualTo(4));
            Assert.That(results[0].F5, Is.EqualTo(5));
            Assert.That(results[0].Hail, Is.EqualTo(6));
            Assert.That(results[0].Wind, Is.EqualTo(7));
            Assert.That(results[0].Year, Is.EqualTo(2010));
            Assert.That(results[0].SourceFk, Is.EqualTo(sourceFk));
            Assert.That(results[0].Id, Is.EqualTo("2010-07-10"));
            Assert.That(results[0].Timestamp, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(results[0].HeadlineEventTime, Is.EqualTo(time));
            Assert.That(results[0].RowCount, Is.EqualTo(28));
            Assert.That(results[0].IsCurrent, Is.False);
        });
    }

    #endregion
}