using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Processes.Models;

namespace OlievortexRed.Tests.ProcessesTests;

public class SatellitePreviewProcessTests
{
    [Test]
    public async Task RunAsync_AllSteps_Valid()
    {
        // Arrange
        const int containerLimit = 1;
        const int channel = 12;
        const int year = 2021;
        const string bucketName = "Dillon";
        const string effectiveDate = "2021-07-18";
        var id = Guid.NewGuid().ToString();
        var ct = CancellationToken.None;
        var satelliteSource = new Mock<ISatelliteSource>();
        satelliteSource.Setup(s => s.GetProductListAsync(effectiveDate, bucketName, channel, ct))
            .ReturnsAsync([new SatelliteAwsProductEntity { Id = id }]);
        var satelliteProcess = new Mock<ISatelliteProcess>();
        satelliteProcess.Setup(s =>
                s.Source1080Async(year, It.IsAny<SatelliteAwsProductEntity>(), null!, null!, null!, null!, ct))
            .ReturnsAsync(true);
        var testable = new SatellitePreviewProcess(satelliteSource.Object, satelliteProcess.Object);
        var request = new SatellitePreviewRequest
        {
            BucketName = bucketName,
            Channel = channel,
            EffectiveDate = effectiveDate,
            Year = year,
            Keys = [id]
        };

        // Act
        await testable.RunAsync(request, null!, containerLimit, null!, null!, null!, null!, ct);

        // Assert
        satelliteSource.Verify(v => v.Start1080ContainersAsync(null!, containerLimit, ct),
            Times.Exactly(1));
    }
}