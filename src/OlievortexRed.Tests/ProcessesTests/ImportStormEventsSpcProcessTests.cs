using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Radar.Interfaces;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.ProcessesTests;

public class ImportStormEventsSpcProcessTests
{
    #region Run

    [Test]
    public async Task Run_CompletesAllSteps_FullRun()
    {
        // Arrange
        var ct = CancellationToken.None;
        var spc = new Mock<ISpcProcess>();
        spc.Setup(s => s.ShouldSkip(It.IsAny<StormEventsSpcInventoryEntity>()))
            .Returns(false);
        spc.Setup(s => s.GetInventoryByYearAsync(It.IsAny<int>(), ct))
            .ReturnsAsync((42, 42, []));
        spc.Setup(s =>
                s.SourceInventoryAsync(It.IsAny<DateTime>(), It.IsAny<List<StormEventsSpcInventoryEntity>>(), ct))
            .ReturnsAsync(new StormEventsSpcInventoryEntity());
        var spcBusiness = new Mock<ISpcBusiness>();
        spcBusiness.Setup(s => s.Parse(It.IsAny<DateTime>(), It.IsAny<string[]>()))
            .Returns([new DailyDetailModel()]);
        var radarSource = new Mock<IRadarSource>();
        radarSource.Setup(s => s.GetPrimaryRadarSitesAsync(ct))
            .ReturnsAsync([]);
        var radarBusiness = new Mock<IRadarBusiness>();
        radarBusiness.Setup(s => s.DownloadInventoryForClosestRadarAsync(It.IsAny<List<RadarSiteEntity>>(),
                It.IsAny<List<RadarInventoryEntity>>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>(),
                null!, ct))
            .ReturnsAsync(new RadarSiteEntity());
        var testable =
            new ImportStormEventsSpcProcess(spc.Object, spcBusiness.Object, radarBusiness.Object, radarSource.Object);

        // Act
        await testable.RunAsync(null!, ct);

        // Assert
        spc.Verify(
            v => v.ProcessEvents(It.IsAny<List<DailyDetailModel>>(), It.IsAny<StormEventsSpcInventoryEntity>(), ct),
            Times.Exactly(16));
    }

    #endregion

    #region ProcessStormReportsForYear

    [Test]
    public async Task ProcessStormReportsForYearAsync_ShortCircuit_WhenShouldSkip()
    {
        // Arrange
        const int year = 2021;
        const int start = 42;
        const int stop = 42;
        var ct = CancellationToken.None;
        var spc = new Mock<ISpcProcess>();
        spc.Setup(s => s.ShouldSkip(It.IsAny<StormEventsSpcInventoryEntity>()))
            .Returns(true);
        spc.Setup(s => s.GetInventoryByYearAsync(year, ct))
            .ReturnsAsync((start, stop, []));
        var testable = new ImportStormEventsSpcProcess(spc.Object, null!, null!, null!);

        // Act
        await testable.ProcessStormReportsForYearAsync(year, null!, ct);

        // Assert
        spc.Verify(
            v => v.ProcessEvents(It.IsAny<List<DailyDetailModel>>(), It.IsAny<StormEventsSpcInventoryEntity>(), ct),
            Times.Exactly(0));
    }

    [Test]
    public async Task ProcessStormReportsForYearAsync_ShortCircuit_DateTooRecent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var year = now.Year;
        var dayNumber = (int)(now - new DateTime(now.Year, 1, 1)).TotalDays;
        var ct = CancellationToken.None;
        var spc = new Mock<ISpcProcess>();
        spc.Setup(s => s.ShouldSkip(It.IsAny<StormEventsSpcInventoryEntity>()))
            .Returns(true);
        spc.Setup(s => s.GetInventoryByYearAsync(year, ct))
            .ReturnsAsync((dayNumber, dayNumber, []));
        var testable = new ImportStormEventsSpcProcess(spc.Object, null!, null!, null!);

        // Act
        await testable.ProcessStormReportsForYearAsync(year, null!, ct);

        // Assert
        spc.Verify(
            v => v.ProcessEvents(It.IsAny<List<DailyDetailModel>>(), It.IsAny<StormEventsSpcInventoryEntity>(), ct),
            Times.Exactly(0));
    }

    [Test]
    public async Task ProcessStormReportsForYearAsync_CompletesAllSteps_YearToProcess()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int year = 2021;
        var spc = new Mock<ISpcProcess>();
        spc.Setup(s => s.ShouldSkip(It.IsAny<StormEventsSpcInventoryEntity>()))
            .Returns(false);
        spc.Setup(s => s.GetInventoryByYearAsync(year, ct))
            .ReturnsAsync((42, 42, []));
        spc.Setup(s =>
                s.SourceInventoryAsync(It.IsAny<DateTime>(), It.IsAny<List<StormEventsSpcInventoryEntity>>(), ct))
            .ReturnsAsync(new StormEventsSpcInventoryEntity());
        var spcBusiness = new Mock<ISpcBusiness>();
        spcBusiness.Setup(s => s.Parse(It.IsAny<DateTime>(), It.IsAny<string[]>()))
            .Returns([]);
        var testable = new ImportStormEventsSpcProcess(spc.Object, spcBusiness.Object, null!, null!);

        // Act
        await testable.ProcessStormReportsForYearAsync(year, null!, ct);

        // Verify
        spc.Verify(
            v => v.ProcessEvents(It.IsAny<List<DailyDetailModel>>(), It.IsAny<StormEventsSpcInventoryEntity>(), ct),
            Times.Exactly(1));
    }

    #endregion
}