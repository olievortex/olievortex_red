using System.Net;
using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormPredictionCenter.Mesos;

namespace OlievortexRed.Tests.StormPredictionCenterTests.MesosTests;

public class MesoProductSourceTests
{
    #region AddToCosmos

    [Test]
    public async Task AddToCosmosAsync_CompletesSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var entity = new SpcMesoProductEntity();

        // Act
        await testable.AddToCosmosAsync(entity, ct);

        // Assert
        cosmos.Verify(v => v.SpcMesoProductCreateAsync(entity, ct), Times.Exactly(1));
    }

    #endregion

    #region DownloadHtml

    [Test]
    public void DownloadHtmlAsync_ThrowsException_NotContent()
    {
        // Arrange
        const int year = 2021;
        const int index = 56;
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), null, ct))
            .ReturnsAsync((HttpStatusCode.NoContent, null, string.Empty));
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(() => testable.DownloadHtmlAsync(year, index, ct));
    }

    [Test]
    public async Task DownloadHtmlAsync_Null_NotFound()
    {
        // Arrange
        const int year = 2021;
        const int index = 56;
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), null, ct))
            .ReturnsAsync((HttpStatusCode.NotFound, null, string.Empty));
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);

        // Act
        var result = await testable.DownloadHtmlAsync(year, index, ct);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DownloadHtmlAsync_Completes_Ok()
    {
        // Arrange
        const int year = 2021;
        const int index = 56;
        var ct = CancellationToken.None;
        var html = Guid.NewGuid().ToString();
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), null, ct))
            .ReturnsAsync((HttpStatusCode.OK, null, html));
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);

        // Act
        var result = await testable.DownloadHtmlAsync(year, index, ct);

        // Assert
        Assert.That(result, Is.EqualTo(html));
    }

    #endregion

    #region DownloadImage

    [Test]
    public async Task DownloadImageAsync_ShortCircuits_AlreadyDownloaded()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var product = new SpcMesoProductEntity
        {
            GraphicUrl = Guid.NewGuid().ToString()
        };

        // Act
        await testable.DownloadImageAsync(string.Empty, product, null!, ct);

        // Assert
        cosmos.Verify(v => v.SpcMesoProductUpdateAsync(product, ct), Times.Never);
    }

    [Test]
    public async Task DownloadImageAsync_Completes_NotDownloaded()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var product = new SpcMesoProductEntity
        {
            EffectiveTime = new DateTime(2021, 7, 18)
        };

        // Act
        await testable.DownloadImageAsync(string.Empty, product, null!, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(product.GraphicUrl, Is.Not.Null);
            Assert.That(product.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
        });
    }

    #endregion

    #region GetFromCosmos

    [Test]
    public async Task GetFromCosmosAsync_CompletesSteps_ValidParameters()
    {
        // Arrange
        const int year = 2023;
        const int index = 743;
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var entity = new SpcMesoProductEntity { Id = 5 };
        cosmos.Setup(s => s.SpcMesoProductGetAsync(year, index, ct))
            .ReturnsAsync(entity);

        // Act
        var result = await testable.GetFromCosmosAsync(year, index, ct);

        // Assert
        Assert.That(result, Is.EqualTo(entity));
    }

    #endregion

    #region GetLatestIdForYear

    [Test]
    public async Task GetLatestIdForYearAsync_CompletesSteps_ValidParameters()
    {
        // Arrange
        const int year = 2023;
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var entity = new SpcMesoProductEntity { Id = 5 };
        cosmos.Setup(s => s.SpcMesoProductGetLatestAsync(year, ct))
            .ReturnsAsync(entity);

        // Act
        var result = await testable.GetLatestIdForYearAsync(year, ct);

        // Assert
        Assert.That(result, Is.EqualTo(5));
    }

    #endregion

    #region UpdateCosmos

    [Test]
    public async Task UpdateCosmosAsync_DoesNothing_NoChange()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var entity = new SpcMesoProductEntity();

        // Act
        await testable.UpdateCosmosAsync(entity, string.Empty, string.Empty, ct);

        // Assert
        cosmos.Verify(v => v.SpcMesoProductUpdateAsync(entity, ct), Times.Never);
    }

    [Test]
    public async Task UpdateCosmosAsync_CompletesSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ows = new Mock<IOlieWebServices>();
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new MesoProductSource(ows.Object, cosmos.Object);
        var entity = new SpcMesoProductEntity();

        // Act
        await testable.UpdateCosmosAsync(entity, "dillon", "tiffany", ct);

        // Assert
        cosmos.Verify(v => v.SpcMesoProductUpdateAsync(entity, ct), Times.Exactly(1));
    }

    #endregion
}