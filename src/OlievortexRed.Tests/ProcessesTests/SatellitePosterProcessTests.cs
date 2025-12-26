using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.StormEvents.Interfaces;

namespace OlievortexRed.Tests.ProcessesTests;

public class SatellitePosterProcessTests
{
    #region Run

    [Test]
    public async Task RunAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        const int containerLimit = 2;
        var ct = CancellationToken.None;
        var process = new Mock<ISatelliteProcess>();
        process.Setup(s =>
                s.Source1080Async(It.IsAny<int>(), It.IsAny<SatelliteAwsProductEntity>(), null!, null!, null!, null!,
                    ct))
            .ReturnsAsync(true);
        process.Setup(s => s.GetSatelliteProductIncompleteAsync(It.IsAny<StormEventsDailySummaryEntity>(), ct))
            .ReturnsAsync(new SatelliteAwsProductEntity());
        var source = new Mock<ISatelliteSource>();
        source.Setup(s => s.GetProductListNoPosterAsync(ct))
            .ReturnsAsync([new SatelliteAwsProductEntity()]);
        var business = new Mock<IDailySummaryBusiness>();
        business.Setup(s => s.GetMissingPostersByYearAsync(It.IsAny<int>(), ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        var testable = new SatellitePosterProcess(process.Object, source.Object, business.Object);

        // Act
        await testable.RunAsync(containerLimit, null!, null!, null!, null!, null!, null!, ct);

        // Assert
        source.Verify(v => v.Start1080ContainersAsync(null!, containerLimit, ct), Times.Exactly(1));
    }

    #endregion

    #region AnnualProcess

    [Test]
    public async Task AnnualProcessAsync_ShortCircuit_NothingToDo()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int year = 2021;
        var process = new Mock<ISatelliteProcess>();
        var source = new Mock<ISatelliteSource>();
        var business = new Mock<IDailySummaryBusiness>();
        business.Setup(s => s.GetMissingPostersByYearAsync(year, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        var testable = new SatellitePosterProcess(process.Object, source.Object, business.Object);

        // Act
        await testable.AnnualProcessAsync(year, null!, null!, null!, null!, null!, ct);

        // Assert
        process.Verify(v => v.Update1080Async(It.IsAny<SatelliteAwsProductEntity>(),
            It.IsAny<StormEventsDailySummaryEntity>(), ct), Times.Never);
    }

    [Test]
    public async Task AnnualProcessAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int year = 2021;
        var process = new Mock<ISatelliteProcess>();
        process.Setup(s => s.GetSatelliteProductIncompleteAsync(It.IsAny<StormEventsDailySummaryEntity>(), ct))
            .ReturnsAsync(new SatelliteAwsProductEntity());
        var source = new Mock<ISatelliteSource>();
        var business = new Mock<IDailySummaryBusiness>();
        business.Setup(s => s.GetMissingPostersByYearAsync(year, ct))
            .ReturnsAsync([new StormEventsDailySummaryEntity()]);
        var testable = new SatellitePosterProcess(process.Object, source.Object, business.Object);

        // Act
        await testable.AnnualProcessAsync(year, null!, null!, null!, null!, null!, ct);

        // Assert
        process.Verify(v => v.Update1080Async(It.IsAny<SatelliteAwsProductEntity>(),
            It.IsAny<StormEventsDailySummaryEntity>(), ct), Times.Exactly(1));
    }

    #endregion
}