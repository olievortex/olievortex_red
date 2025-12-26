using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.ProcessesTests;

public class ImportStormEventsDatabaseProcessTests
{
    #region Run

    [Test]
    public async Task RunAsync_False_NothingToDo()
    {
        // Arrange
        const int year = 2021;
        const string id = "20250401";
        var ct = CancellationToken.None;
        var process = new Mock<IDatabaseProcess>();
        process.Setup(s => s.LoadAsync(null!, It.IsAny<StormEventsDatabaseInventoryEntity>(), ct))
            .ReturnsAsync([]);
        var databaseBusiness = new Mock<IDatabaseBusiness>();
        databaseBusiness.Setup(s => s.DatabaseGetInventoryAsync(year, id, ct))
            .ReturnsAsync(new StormEventsDatabaseInventoryEntity());
        var radarSource = new Mock<IRadarSource>();
        var radarBusiness = new Mock<IRadarBusiness>();
        var testable = new ImportStormEventsDatabaseProcess(process.Object, databaseBusiness.Object, radarSource.Object,
            radarBusiness.Object);

        // Act
        var result = await testable.RunAsync(year, id, null!, null!, ct);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RunAsync_True_TooMany()
    {
        // Arrange
        const int year = 2021;
        const string sourceFk = "20250401";
        var ct = CancellationToken.None;
        var work = new List<DailyDetailModel>();
        for (var i = 0; i < 40; i++)
            work.Add(new DailyDetailModel { Effective = new DateTime(2021, i / 5 + 1, i % 20 + 1, 0, 0, 0) });
        var process = new Mock<IDatabaseProcess>();
        process.Setup(s => s.LoadAsync(null!, It.IsAny<StormEventsDatabaseInventoryEntity>(), ct))
            .ReturnsAsync(work);
        process.Setup(s => s.DeactivateOldSummariesAsync(It.IsAny<string>(), year, sourceFk, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        process.Setup(s => s.GetAggregate(It.IsAny<List<DailyDetailModel>>()))
            .Returns([new DailySummaryModel()]);
        var databaseBusiness = new Mock<IDatabaseBusiness>();
        databaseBusiness.Setup(s => s.DatabaseGetInventoryAsync(year, sourceFk, ct))
            .ReturnsAsync(new StormEventsDatabaseInventoryEntity());
        var radarSource = new Mock<IRadarSource>();
        var radarBusiness = new Mock<IRadarBusiness>();
        radarBusiness.Setup(s => s.DownloadInventoryForClosestRadarAsync(It.IsAny<List<RadarSiteEntity>>(),
                It.IsAny<List<RadarInventoryEntity>>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>(),
                null!, ct))
            .ReturnsAsync(new RadarSiteEntity());
        var testable = new ImportStormEventsDatabaseProcess(process.Object, databaseBusiness.Object, radarSource.Object,
            radarBusiness.Object);

        // Act
        var result = await testable.RunAsync(year, sourceFk, null!, null!, ct);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task RunAsync_True_JustOne()
    {
        // Arrange
        const int year = 2021;
        const string sourceFk = "20250401";
        var ct = CancellationToken.None;
        var work = new List<DailyDetailModel> { new() { Effective = new DateTime(2021, 1, 1, 12, 0, 0) } };
        var process = new Mock<IDatabaseProcess>();
        process.Setup(s => s.LoadAsync(null!, It.IsAny<StormEventsDatabaseInventoryEntity>(), ct))
            .ReturnsAsync(work);
        process.Setup(s => s.DeactivateOldSummariesAsync(It.IsAny<string>(), year, sourceFk, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity { SourceFk = sourceFk }]);
        var databaseBusiness = new Mock<IDatabaseBusiness>();
        databaseBusiness.Setup(s => s.DatabaseGetInventoryAsync(year, sourceFk, ct))
            .ReturnsAsync(new StormEventsDatabaseInventoryEntity());
        var radarSource = new Mock<IRadarSource>();
        var radarBusiness = new Mock<IRadarBusiness>();
        var testable = new ImportStormEventsDatabaseProcess(process.Object, databaseBusiness.Object, radarSource.Object,
            radarBusiness.Object);

        // Act
        var result = await testable.RunAsync(year, sourceFk, null!, null!, ct);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void RunAsync_Exception_BadAggregate()
    {
        // Arrange
        const int year = 2021;
        const string sourceFk = "20250401";
        var ct = CancellationToken.None;
        var work = new List<DailyDetailModel>();
        for (var i = 0; i < 40; i++)
            work.Add(new DailyDetailModel { Effective = new DateTime(2021, i / 5 + 1, i % 20 + 1, 0, 0, 0) });
        var process = new Mock<IDatabaseProcess>();
        process.Setup(s => s.LoadAsync(null!, It.IsAny<StormEventsDatabaseInventoryEntity>(), ct))
            .ReturnsAsync(work);
        process.Setup(s => s.DeactivateOldSummariesAsync(It.IsAny<string>(), year, sourceFk, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        process.Setup(s => s.GetAggregate(It.IsAny<List<DailyDetailModel>>()))
            .Returns([new DailySummaryModel(), new DailySummaryModel()]);
        var databaseBusiness = new Mock<IDatabaseBusiness>();
        databaseBusiness.Setup(s => s.DatabaseGetInventoryAsync(year, sourceFk, ct))
            .ReturnsAsync(new StormEventsDatabaseInventoryEntity());
        var radarSource = new Mock<IRadarSource>();
        var radarBusiness = new Mock<IRadarBusiness>();
        radarBusiness.Setup(s => s.DownloadInventoryForClosestRadarAsync(It.IsAny<List<RadarSiteEntity>>(),
                It.IsAny<List<RadarInventoryEntity>>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>(),
                null!, ct))
            .ReturnsAsync(new RadarSiteEntity());
        var testable = new ImportStormEventsDatabaseProcess(process.Object, databaseBusiness.Object, radarSource.Object,
            radarBusiness.Object);

        // Act, Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => testable.RunAsync(year, sourceFk, null!, null!, ct));
    }

    #endregion
}