using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.StormEvents.Interfaces;

namespace OlievortexRed.Tests.ProcessesTests;

public class SatelliteInventoryProcessTests
{
    #region GetMissingDays

    [Test]
    public async Task GetMissingDays_ReturnsDays_ValidParameters()
    {
        // Arrange
        const int year = 2021;
        var ct = CancellationToken.None;
        var business = new Mock<IDailySummaryBusiness>();
        business.Setup(s => s.GetSevereByYearAsync(year, ct))
            .ReturnsAsync([
                    new StormEventsDailySummaryEntity { Id = "2021-07-10" },
                    new StormEventsDailySummaryEntity { Id = "2021-07-11" }
                ]
            );
        var process = new Mock<ISatelliteProcess>();
        var source = new Mock<ISatelliteSource>();
        source.Setup(s => s.GetInventoryByYearAsync(year, It.IsAny<int>(), It.IsAny<DayPartsEnum>(), ct))
            .ReturnsAsync([new SatelliteAwsInventoryEntity { EffectiveDate = "2021-07-10" }]);
        var testable = new SatelliteInventoryProcess(business.Object, process.Object, source.Object);

        // Act
        var result = await testable.GetMissingDaysAsync(year, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo("2021-07-11"));
        });
    }

    #endregion

    #region ProcessYear

    [Test]
    public async Task ProcessYearAsync_CompletesAllSteps_RecentYear()
    {
        // Arrange
        const int year = 2022;
        const int channel = 2;
        const DayPartsEnum dayPart = DayPartsEnum.Afternoon;
        var ct = CancellationToken.None;
        var stormy = new Mock<IDailySummaryBusiness>();
        stormy.Setup(s => s.GetSevereByYearAsync(year, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        var process = new Mock<ISatelliteProcess>();
        var source = new Mock<ISatelliteSource>();
        source.Setup(s => s.GetInventoryByYearAsync(year, channel, dayPart, ct))
            .ReturnsAsync([]);
        var testable = new SatelliteInventoryProcess(stormy.Object, process.Object, source.Object);

        // Act
        await testable.ProcessYearAsync(null!, year, ct);

        // Assert
        source.Verify(v => v.GetInventoryByYearAsync(year, channel, dayPart, ct), Times.Exactly(1));
    }

    #endregion

    #region RunAsync

    [Test]
    public async Task RunAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        const int channel = 2;
        const DayPartsEnum dayPart = DayPartsEnum.Afternoon;
        var ct = CancellationToken.None;
        var stormy = new Mock<IDailySummaryBusiness>();
        stormy.Setup(s => s.GetSevereByYearAsync(It.IsAny<int>(), ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        var process = new Mock<ISatelliteProcess>();
        var source = new Mock<ISatelliteSource>();
        source.Setup(s => s.GetInventoryByYearAsync(It.IsAny<int>(), channel, dayPart, ct))
            .ReturnsAsync([]);
        var testable = new SatelliteInventoryProcess(stormy.Object, process.Object, source.Object);

        // Act
        await testable.RunAsync(null!, ct);

        // Assert
        source.Verify(v => v.GetInventoryByYearAsync(It.IsAny<int>(), channel, dayPart, ct), Times.Exactly(16));
    }

    #endregion
}