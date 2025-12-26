using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;
using OlievortexRed.Lib.StormPredictionCenter.Mesos;

namespace OlievortexRed.Tests.StormPredictionCenterTests.MesosTests;

public class MesoProductProcessTests
{
    #region Download

    [Test]
    public async Task DownloadAsync_ShortCircuit_NoFile()
    {
        // Arrange
        const int year = 2023;
        const int index = 253;
        var ct = CancellationToken.None;
        var source = new Mock<IMesoProductSource>();
        var testable = new MesoProductProcess(source.Object, null!);

        // Act
        var result = await testable.DownloadAsync(year, index, null!, ct);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DownloadAsync_CompletesSteps_NewFile()
    {
        // Arrange
        const int year = 2023;
        const int index = 253;
        var entity = new SpcMesoProductEntity();
        var effectiveTime = new DateTime(2023, 3, 15, 21, 35, 0);
        var areasAffected = Guid.NewGuid().ToString();
        var concerning = Guid.NewGuid().ToString();
        var narrative = Guid.NewGuid().ToString();
        var html = Guid.NewGuid().ToString();
        var ct = CancellationToken.None;
        var source = new Mock<IMesoProductSource>();
        source.Setup(s => s.AddToCosmosAsync(It.IsAny<SpcMesoProductEntity>(), ct))
            .Callback((SpcMesoProductEntity e, CancellationToken _) => entity = e);
        source.Setup(s => s.DownloadHtmlAsync(year, index, ct)).ReturnsAsync(html);
        var parse = new Mock<IMesoProductParsing>();
        parse.Setup(s => s.GetEffectiveTime(It.IsAny<string>())).Returns(effectiveTime);
        parse.Setup(s => s.GetAreasAffected(It.IsAny<string>())).Returns(areasAffected);
        parse.Setup(s => s.GetConcerning(It.IsAny<string>())).Returns(concerning);
        parse.Setup(s => s.GetNarrative(It.IsAny<string>())).Returns(narrative);
        var testable = new MesoProductProcess(source.Object, parse.Object);

        // Act
        var result = await testable.DownloadAsync(year, index, null!, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(entity.AreasAffected, Is.EqualTo(areasAffected));
            Assert.That(entity.EffectiveDate, Is.EqualTo("2023-03-15"));
            Assert.That(entity.Concerning, Is.EqualTo(concerning));
            Assert.That(entity.Narrative, Is.EqualTo(narrative));
            Assert.That(entity.Html, Is.EqualTo(html));
        });
    }

    #endregion

    #region GetCurrentMdIndex

    [Test]
    public async Task GetCurrentMdIndexAsync_CompletesSteps_ValidParameters()
    {
        // Arrange
        const int year = 2021;
        const int id = 23;
        var ct = CancellationToken.None;
        var source = new Mock<IMesoProductSource>();
        source.Setup(s => s.GetLatestIdForYearAsync(year, ct))
            .ReturnsAsync(id);
        var testable = new MesoProductProcess(source.Object, null!);

        // Act
        var result = await testable.GetCurrentMdIndexAsync(year, ct);

        // Assert
        Assert.That(result, Is.EqualTo(id));
    }

    #endregion

    #region Update

    [Test]
    public async Task UpdateAsync_ShortCircuit_NoRecord()
    {
        // Arrange
        const int year = 2023;
        const int index = 253;
        var ct = CancellationToken.None;
        var source = new Mock<IMesoProductSource>();
        var testable = new MesoProductProcess(source.Object, null!);

        // Act
        var result = await testable.UpdateAsync(year, index, ct);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateAsync_CompletesSteps_ExistingRecord()
    {
        // Arrange
        const int year = 2023;
        const int index = 253;
        var entity = new SpcMesoProductEntity();
        var ct = CancellationToken.None;
        var source = new Mock<IMesoProductSource>();
        source.Setup(s => s.GetFromCosmosAsync(year, index, ct)).ReturnsAsync(entity);
        var parse = new Mock<IMesoProductParsing>();
        var testable = new MesoProductProcess(source.Object, parse.Object);

        // Act
        var result = await testable.UpdateAsync(year, index, ct);

        // Assert
        Assert.Multiple(() => { Assert.That(result, Is.True); });
    }

    #endregion
}