using Amazon.S3;
using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Enums;
using OlievortexRed.Lib.Maps.Interfaces;
using OlievortexRed.Lib.Maps.Satellite;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Tests.MapsTests.SatelliteTests;

public class SatelliteAwsBusinessTests
{
    #region Download

    [Test]
    public async Task DownloadAsync_ShortCircuit_AlreadyDownloaded()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var awsSource = new Mock<ISatelliteAwsSource>();
        var source = new Mock<ISatelliteSource>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new SatelliteAwsBusiness(ows.Object, awsSource.Object, source.Object, cosmos.Object);
        var product = new SatelliteAwsProductEntity
        {
            PathSource = "a"
        };

        // Act
        await testable.DownloadAsync(product, Delay, null!, null!, ct);

        // Assert
        Assert.That(product.Timestamp, Is.EqualTo(DateTime.MinValue));
    }

    [Test]
    public async Task DownloadAsync_Retries_OneFailure()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        ows.SetupSequence(s => s.AwsDownloadAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), null!, ct))
            .Returns(Task.FromException(new AmazonS3Exception("Olie")))
            .Returns(Task.CompletedTask);
        var awsSource = new Mock<ISatelliteAwsSource>();
        var source = new Mock<ISatelliteSource>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new SatelliteAwsBusiness(ows.Object, awsSource.Object, source.Object, cosmos.Object);
        var product = new SatelliteAwsProductEntity();

        // Act
        await testable.DownloadAsync(product, Delay, null!, null!, ct);

        // Assert
        Assert.That(product.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void DownloadAsync_Trows_ThreeFailures()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.AwsDownloadAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), null!, ct))
            .Returns(Task.FromException(new AmazonS3Exception("Olie")));
        var awsSource = new Mock<ISatelliteAwsSource>();
        var source = new Mock<ISatelliteSource>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new SatelliteAwsBusiness(ows.Object, awsSource.Object, source.Object, cosmos.Object);
        var product = new SatelliteAwsProductEntity();

        // Act, Assert
        Assert.ThrowsAsync<AmazonS3Exception>(() => testable.DownloadAsync(product, Delay, null!, null!, ct));
    }

    private static Task Delay(int _)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region ListAwsKeys

    [Test]
    public async Task ListAwsKeysAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.AwsListAsync(null!, null!, null!, ct))
            .ReturnsAsync(["a", "b"]);
        const string dayValue = "2021-05-18";
        const DayPartsEnum dayPart = DayPartsEnum.Afternoon;
        const int satellite = 16;
        const int channel = 13;
        var effectiveDate = new DateTime(2021, 5, 18);
        var awsSource = new Mock<ISatelliteAwsSource>();
        awsSource.Setup(s => s.GetChannelFromAwsKey(It.IsAny<string>())).Returns(channel);
        var source = new Mock<ISatelliteSource>();
        source.Setup(s => s.GetEffectiveDate(dayValue)).Returns(effectiveDate);
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new SatelliteAwsBusiness(ows.Object, awsSource.Object, source.Object, cosmos.Object);

        // Act
        var result = await testable.ListAwsKeysAsync(dayValue, satellite, channel, dayPart, null!, ct);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Keys, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task ListAwsKeysAsync_ShortCircuit_OldYear()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        const string dayValue = "2021-05-18";
        const DayPartsEnum dayPart = DayPartsEnum.Afternoon;
        const int satellite = 16;
        const int channel = 13;
        var awsSource = new Mock<ISatelliteAwsSource>();
        var source = new Mock<ISatelliteSource>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new SatelliteAwsBusiness(ows.Object, awsSource.Object, source.Object, cosmos.Object);

        // Act
        var result = await testable.ListAwsKeysAsync(dayValue, satellite, channel, dayPart, null!, ct);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion
}