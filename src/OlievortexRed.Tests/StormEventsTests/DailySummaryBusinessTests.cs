using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.StormEventsTests;

public class DailySummaryBusinessTests
{
    #region AggregateByDay

    [Test]
    public void AggregateByDay_ThrowsException_DateTimeNotUtc()
    {
        // Arrange
        var records = new List<DailyDetailModel>
        {
            new() { Effective = new DateTime(2021, 7, 10) }
        };

        // Act, Assert
        Assert.Throws<ArgumentException>(() => DailySummaryBusiness.AggregateByDate(records));
    }

    [Test]
    public void AggregateByDate_CorrectSort_ValidInput()
    {
        // Arrange
        var records = new List<DailyDetailModel>
        {
            CreateTornadoTest(2),
            CreateTornadoTest(0)
        };

        // Act
        var result = DailySummaryBusiness.AggregateByDate(records);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].EffectiveDate, Is.LessThan(result[1].EffectiveDate));
        });

        return;

        static DailyDetailModel CreateTornadoTest(int dayNumber)
        {
            return new DailyDetailModel
            {
                Effective = new DateTime(2021, 7, 10, 0, 0, 0, DateTimeKind.Utc).AddDays(dayNumber),
                EventType = "Tornado",
                Magnitude = "EF2",
                State = "Minnesota",
                County = "Anoka",
                City = "Blaine"
            };
        }
    }

    [Test]
    public void AggregateByDate_CorrectTally_ValidInput()
    {
        // Arrange
        var timeScratchPad = new DateTime(2021, 7, 10, 23, 12, 4, DateTimeKind.Utc);
        var expected = new DateTime(2021, 7, 10, 22, 57, 4);
        var records = new List<DailyDetailModel>
        {
            CreateTornadoTest("EF5"),
            CreateTornadoTest("EF4"),
            CreateTornadoTest("EF3"),
            CreateTornadoTest("EF2"),
            CreateTornadoTest("EF1"),
            CreateTornadoTest("EF0"),
            CreateTornadoTest("EFU"),
            CreateOtherTest("Thunderstorm Wind"),
            CreateOtherTest("Hail")
        };

        // Act
        var result = DailySummaryBusiness.AggregateByDate(records);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].EffectiveDate, Is.EqualTo("2021-07-10"));
            Assert.That(result[0].F1, Is.EqualTo(3));
            Assert.That(result[0].F2, Is.EqualTo(1));
            Assert.That(result[0].F3, Is.EqualTo(1));
            Assert.That(result[0].F4, Is.EqualTo(1));
            Assert.That(result[0].F5, Is.EqualTo(1));
            Assert.That(result[0].Hail, Is.EqualTo(1));
            Assert.That(result[0].Wind, Is.EqualTo(1));
            Assert.That(result[0].HeadlineEventTime, Is.EqualTo(expected));
        });

        return;

        DailyDetailModel CreateTornadoTest(string magnitude)
        {
            timeScratchPad = timeScratchPad.AddMinutes(-15);

            return new DailyDetailModel
            {
                Effective = timeScratchPad,
                EventType = "Tornado",
                Magnitude = magnitude
            };
        }

        DailyDetailModel CreateOtherTest(string eventType)
        {
            timeScratchPad = timeScratchPad.AddMinutes(-15);

            return new DailyDetailModel
            {
                Effective = timeScratchPad,
                EventType = eventType
            };
        }
    }

    #endregion

    #region DecodeHeadlineScore

    [Test]
    public void DecodeHeadlineScore_Null_Null()
    {
        // Arrange, Act
        var result = DailySummaryBusiness.DecodeHeadlineScore(null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void DecodeHeadlineScore_Null_StartsWith9()
    {
        // Arrange
        const string value = "9";

        // Act
        var result = DailySummaryBusiness.DecodeHeadlineScore(value);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void DecodeHeadlineScore_Time_Valid()
    {
        // Arrange
        const string value = "72021-07-10 18:13:43Z";
        var expected = new DateTime(2021, 7, 10, 18, 13, 43);

        // Act
        var result = DailySummaryBusiness.DecodeHeadlineScore(value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region EncodeHeadlineScore

    [Test]
    public void EncodeHeadlineScore_9_EarlyTime()
    {
        // Arrange
        var effective = new DateTime(2021, 7, 10, 13, 13, 43);
        var model = new DailyDetailModel { Effective = effective };

        // Act
        var result = DailySummaryBusiness.EncodeHeadlineScore(model);

        // Assert
        Assert.That(result, Is.EqualTo("9"));
    }

    [Test]
    public void EncodeHeadlineScore_8_UnknownEvent()
    {
        // Arrange
        var effective = new DateTime(2021, 7, 10, 19, 13, 43);
        var model = new DailyDetailModel { Effective = effective, EventType = "Meow" };

        // Act
        var result = DailySummaryBusiness.EncodeHeadlineScore(model);

        // Assert
        Assert.That(result, Is.EqualTo("82021-07-10 19:13:43Z"));
    }

    [Test]
    public void EncodeHeadlineScore_Score_ValidEvent()
    {
        // Arrange
        var effective = new DateTime(2021, 7, 10, 18, 13, 43);
        var model = new DailyDetailModel { Effective = effective, EventType = "Hail" };

        // Act
        var result = DailySummaryBusiness.EncodeHeadlineScore(model);

        // Assert
        Assert.That(result, Is.EqualTo("72021-07-10 18:13:43Z"));
    }

    #endregion

    #region GetMissingPostersByYear

    [Test]
    public async Task GetMissingPostersByYearAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new DailySummaryBusiness(cosmos.Object);
        var expected = new List<StormEventsDailySummaryEntity>();
        const int year = 2021;
        var ct = CancellationToken.None;
        cosmos.Setup(s => s.StormEventsDailySummaryListMissingPostersForYear(year, ct))
            .ReturnsAsync(expected);

        // Act
        var result = await testable.GetMissingPostersByYearAsync(year, ct);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetSevereByYear

    [Test]
    public async Task GetSevereByYearAsync_ReturnsTopRow_ValidParameters()
    {
        // Arrange
        const int year = 2021;
        const string date1 = "2021-07-10";
        const string date2 = "2021-07-11";
        var now = DateTime.UtcNow;
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new DailySummaryBusiness(cosmos.Object);
        var entities = new List<StormEventsDailySummaryEntity>
        {
            new() { Id = date1, Year = year, SourceFk = "a", Timestamp = now.AddMonths(-1) },
            new() { Id = date1, Year = year, SourceFk = "b", Timestamp = now },
            new() { Id = date2, Year = year, SourceFk = "c", Timestamp = now }
        };
        cosmos.Setup(s => s.StormEventsDailySummaryListSevereForYear(year, ct))
            .ReturnsAsync(entities);

        // Act
        var result = await testable.GetSevereByYearAsync(year, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].SourceFk, Is.EqualTo("b"));
            Assert.That(result[1].SourceFk, Is.EqualTo("c"));
        });
    }

    #endregion

    #region UpdateCosmos

    [Test]
    public async Task UpdateCosmosAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new DailySummaryBusiness(cosmos.Object);
        var ct = CancellationToken.None;
        var product = new StormEventsDailySummaryEntity();

        // Act
        await testable.UpdateCosmosAsync(product, ct);

        // Assert
        cosmos.Verify(v => v.StormEventsDailySummaryUpdateAsync(product, ct),
            Times.Exactly(1));
    }

    #endregion
}