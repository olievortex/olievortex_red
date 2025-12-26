using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.StormEventsTests;

public class SpcBusinessTests
{
    #region AddDailyDetail

    [Test]
    public async Task DailyDetailAsync_ShortCircuits_AlreadyComplete()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity
        {
            IsDailyDetailComplete = true
        };
        var models = new List<DailyDetailModel>();

        // Act
        await testable.AddDailyDetailAsync(models, inventory, ct);

        // Assert
        cosmos.Verify(v => v.StormEventsSpcInventoryUpdateAsync(inventory, ct), Times.Never);
    }

    [Test]
    public async Task DailyDetailAsync_CompletesAllSteps_New()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity();
        var models = new List<DailyDetailModel>();

        // Act
        await testable.AddDailyDetailAsync(models, inventory, ct);

        // Assert
        Assert.That(inventory.IsDailyDetailComplete, Is.True);
    }

    #endregion

    #region AddDailySummary

    [Test]
    public async Task DailySummaryAsync_ShortCircuit_AlreadyComplete()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity
        {
            IsDailySummaryComplete = true
        };

        // Act
        await testable.AddDailySummaryAsync(inventory, null, string.Empty, ct);

        // Assert
        cosmos.Verify(v => v.StormEventsSpcInventoryUpdateAsync(inventory, ct), Times.Never);
    }

    [Test]
    public async Task DailySummaryAsync_ShortCircuit_NullModel()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity();

        // Act
        await testable.AddDailySummaryAsync(inventory, null, string.Empty, ct);

        // Assert
        cosmos.Verify(v => v.StormEventsSpcInventoryUpdateAsync(inventory, ct), Times.Never);
    }

    [Test]
    public async Task DailySummaryAsync_CompletesAllSteps_New()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        cosmos.Setup(s => s.StormEventsDailySummaryListSummariesForDate("2010-07-18", 2010, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity { IsCurrent = true }]);
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity
        {
            EffectiveDate = "2010-07-18"
        };
        var model = new DailySummaryModel
        {
            EffectiveDate = "2010-07-18",
            F1 = 1
        };

        // Act
        await testable.AddDailySummaryAsync(inventory, model, string.Empty, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(inventory.IsDailySummaryComplete, Is.True);
            Assert.That(inventory.IsTornadoDay, Is.True);
        });
    }

    #endregion

    #region DownloadNew

    [Test]
    public async Task DownloadNewAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        source.Setup(s => s.DownloadNewAsync(effectiveDate, ct)).ReturnsAsync(("a", "b"));
        var testable = new SpcBusiness(cosmos.Object, source.Object);

        // Act
        var result = await testable.DownloadNewAsync(effectiveDate, ct);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    #endregion

    #region DownloadUpdate

    [Test]
    public async Task DownloadUpdateAsync_ShortCircuits_TooRecent()
    {
        // Arrange
        var ct = CancellationToken.None;
        var timestamp = DateTime.UtcNow.AddDays(-1);
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity { Timestamp = timestamp };

        // Act
        var result = await testable.DownloadUpdateAsync(inventory, ct);

        // Assert
        Assert.That(result.Timestamp, Is.EqualTo(timestamp));
    }

    [Test]
    public async Task DownloadUpdateAsync_ShortCircuits_NotATornadoDay()
    {
        // Arrange
        var ct = CancellationToken.None;
        var timestamp = DateTime.UtcNow.AddMonths(-1);
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity { Timestamp = timestamp };

        // Act
        var result = await testable.DownloadUpdateAsync(inventory, ct);

        // Assert
        Assert.That(result.Timestamp, Is.EqualTo(timestamp));
    }

    [Test]
    public async Task DownloadUpdateAsync_ShortCircuits_NotModified()
    {
        // Arrange
        var ct = CancellationToken.None;
        var timestamp = DateTime.UtcNow.AddMonths(-1);
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        source.Setup(s => s.DownloadUpdateAsync(It.IsAny<DateTime>(), It.IsAny<string>(), ct))
            .ReturnsAsync((string.Empty, string.Empty, false));
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity
        {
            Id = Guid.NewGuid().ToString(),
            EffectiveDate = "2010-07-10",
            Timestamp = timestamp,
            IsTornadoDay = true
        };

        // Act
        var result = await testable.DownloadUpdateAsync(inventory, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsTornadoDay, Is.True);
            Assert.That(result.Timestamp, Is.Not.EqualTo(timestamp));
        });
    }

    [Test]
    public async Task DownloadUpdateAsync_Downloads_Update()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        source.Setup(s => s.DownloadUpdateAsync(It.IsAny<DateTime>(), It.IsAny<string>(), ct))
            .ReturnsAsync(("a\nb", "c", true));
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var inventory = new StormEventsSpcInventoryEntity
        {
            Id = Guid.NewGuid().ToString(),
            EffectiveDate = "2010-07-10",
            Timestamp = DateTime.UtcNow.AddMonths(-1),
            IsTornadoDay = true
        };

        // Act
        var result = await testable.DownloadUpdateAsync(inventory, ct);

        // Assert
        Assert.That(result.IsTornadoDay, Is.False);
    }

    #endregion

    #region Parse

    [Test]
    public void Parse_Parses_ValidParameters()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        string[] lines = [];
        var expected = new List<DailyDetailModel>();
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        source.Setup(s => s.Parse(effectiveDate, lines)).Returns(expected);

        // Act
        var result = testable.Parse(effectiveDate, lines);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetAggregate

    [Test]
    public void GetAggregate_ThrowsException_MultipleDays()
    {
        // Arrange
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var models = new List<DailyDetailModel>
        {
            new() { Effective = new DateTime(2010, 5, 10, 0, 0, 0, DateTimeKind.Utc) },
            new() { Effective = new DateTime(2010, 6, 12, 0, 0, 0, DateTimeKind.Utc) }
        };
        var testable = new SpcBusiness(cosmos.Object, source.Object);

        // Act, Assert
        Assert.Throws<Exception>(() => testable.GetAggregate(models));
    }

    [Test]
    public void GetAggregate_Null_NoEvents()
    {
        // Arrange
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var models = new List<DailyDetailModel>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);

        // Act
        var result = testable.GetAggregate(models);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAggregate_NotNull_Events()
    {
        // Arrange
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var models = new List<DailyDetailModel>
        {
            new() { Effective = new DateTime(2010, 5, 10, 0, 0, 0, DateTimeKind.Utc) },
            new() { Effective = new DateTime(2010, 5, 10, 0, 0, 0, DateTimeKind.Utc) }
        };
        var testable = new SpcBusiness(cosmos.Object, source.Object);

        // Act
        var result = testable.GetAggregate(models);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    #endregion

    #region GetFirstDayNumberForYear

    [Test]
    public void GetFirstDayNumberForYear_MaxValue_EventsDatabaseEra()
    {
        // Arrange
        const int year = 2018;

        // Act
        var result = SpcBusiness.GetFirstDayNumberForYear(year);

        // Act
        Assert.That(result, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void GetFirstDayNumberForYear_StartOfYear_NotEventsDatabaseEra()
    {
        // Arrange
        const int year = 2025;
        var startOfYear = new DateTime(2025, 1, 1);

        // Act
        var result = startOfYear.AddDays(SpcBusiness.GetFirstDayNumberForYear(year));

        // Act
        Assert.That(result, Is.EqualTo(startOfYear));
    }

    #endregion

    #region GetLastDayNumberForYear

    [Test]
    public void GetLastDayNumberForYear_LastDay_NotLeapYear()
    {
        // Arrange
        const int year = 2018;
        var startOfYear = new DateTime(2018, 1, 1);
        var expected = new DateTime(2018, 12, 31);

        // Act
        var result = startOfYear.AddDays(SpcBusiness.GetLastDayNumberForYear(year));

        // Act
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetLastDayNumberForYear_LastDay_LeapYear()
    {
        // Arrange
        const int year = 2020;
        var startOfYear = new DateTime(2020, 1, 1);
        var expected = new DateTime(2020, 12, 31);

        // Act
        var result = startOfYear.AddDays(SpcBusiness.GetLastDayNumberForYear(year));

        // Act
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetInventoryByYear

    [Test]
    public async Task GetInventoryByYearAsync_CallsAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int year = 2021;
        var cosmos = new Mock<ICosmosRepository>();
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);
        var expected = new List<StormEventsSpcInventoryEntity>();
        cosmos.Setup(s => s.StormEventsSpcInventoryListByYearAsync(year, ct))
            .ReturnsAsync(expected);

        // Act
        var result = await testable.GetInventoryByYearAsync(year, ct);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    [Test]
    public void GetLatest_ReturnsTopRow_MultipleRecords()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        var cosmos = new Mock<ICosmosRepository>();
        var inventory = new List<StormEventsSpcInventoryEntity>
        {
            new() { Id = "a", Timestamp = DateTime.UtcNow.AddDays(-2), EffectiveDate = "2021-07-10" },
            new() { Id = "b", Timestamp = DateTime.UtcNow, EffectiveDate = "2021-07-10" }
        };
        var source = new Mock<ISpcSource>();
        var testable = new SpcBusiness(cosmos.Object, source.Object);

        // Act
        var result = testable.GetLatest(effectiveDate, inventory);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("b"));
    }
}