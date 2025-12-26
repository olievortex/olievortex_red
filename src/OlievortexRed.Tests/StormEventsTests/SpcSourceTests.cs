using Moq;
using OlievortexRed.Lib.Services;
using OlievortexRed.Lib.StormEvents;
using System.Net;
using System.Net.Http.Headers;

namespace OlievortexRed.Tests.StormEventsTests;

public class SpcSourceTests
{
    private readonly string _csv = File.ReadAllText("./Resources/StormEvents/250105_rpts_filtered.csv");

    #region DownloadNew

    [Test]
    public void DownloadNewAsync_ThrowsException_Redirect()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.Redirect, null, string.Empty));
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.ThrowsAsync<Exception>(() => testable.DownloadNewAsync(effectiveDate, ct));
    }

    [Test]
    public void DownloadNewAsync_ThrowsException_NoEtag()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.OK, null, string.Empty));
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.ThrowsAsync<Exception>(() => testable.DownloadNewAsync(effectiveDate, ct));
    }

    [Test]
    public async Task DownloadNewAsync_Downloads_New()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2010, 7, 10);
        var etag = new EntityTagHeaderValue("\"Dillon\"");
        var body = Guid.NewGuid().ToString();
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.OK, etag, body));
        var testable = new SpcSource(ows.Object);

        // Act
        var (a, b) = await testable.DownloadNewAsync(effectiveDate, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(body));
            Assert.That(b, Is.EqualTo(etag.Tag));
        });
    }

    #endregion

    #region DownloadUpdate

    [Test]
    public async Task DownloadUpdateAsync_ShortCircuits_NotModified()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        const string etag = "\"a\"";
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.NotModified, null, string.Empty));
        var testable = new SpcSource(ows.Object);

        // Act
        var (_, __, result) = await testable.DownloadUpdateAsync(effectiveDate, etag, ct);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void DownloadUpdateAsync_ThrowsException_NoETag()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        const string etag = "\"a\"";
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.OK, null, string.Empty));
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(() => testable.DownloadUpdateAsync(effectiveDate, etag, ct));
    }

    [Test]
    public void DownloadUpdateAsync_ThrowsException_NotFound()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        const string etag = "\"a\"";
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.NotFound, null, string.Empty));
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.ThrowsAsync<ApplicationException>(() => testable.DownloadUpdateAsync(effectiveDate, etag, ct));
    }

    [Test]
    public async Task DownloadUpdateAsync_Downloads_Update()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2021, 7, 10);
        const string etag = "\"a\"";
        var etagValue = new EntityTagHeaderValue("\"Dillon\"");
        var body = Guid.NewGuid().ToString();
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetAsync(It.IsAny<string>(), It.IsAny<EntityTagHeaderValue>(), ct))
            .ReturnsAsync((HttpStatusCode.OK, etagValue, body));
        var testable = new SpcSource(ows.Object);

        // Act
        var (a, b, c) = await testable.DownloadUpdateAsync(effectiveDate, etag, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(body));
            Assert.That(b, Is.EqualTo("\"Dillon\""));
            Assert.That(c, Is.True);
        });
    }

    #endregion

    #region GetUrl

    [Test]
    public void GetUrl_ReturnsUrl_DateTime()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);

        // Act
        var result = SpcSource.GetUrl(effectiveDate);

        // Assert
        Assert.That(result, Is.EqualTo("https://www.spc.noaa.gov/climo/reports/210710_rpts_filtered.csv"));
    }

    #endregion

    #region Parse

    [Test]
    public void Parse_ReturnsList_ValidFormat()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        var lines = _csv.ReplaceLineEndings("\n").Split('\n');
        var ows = new Mock<IOlieWebServices>();
        var testable = new SpcSource(ows.Object);

        // Act
        var result = testable.Parse(effectiveDate, lines);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(55));
            Assert.That(result[2].Magnitude, Is.EqualTo("EF2"));
            Assert.That(result[17].Magnitude, Is.EqualTo("55"));
            Assert.That(result[52].Magnitude, Is.EqualTo("1.00"));
        });
    }

    [Test]
    public void Parse_ThrowsException_BadFormat()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        string[] lines = ["a,b,c,d"];
        var ows = new Mock<IOlieWebServices>();
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.Throws<Exception>(() => testable.Parse(effectiveDate, lines));
    }

    [Test]
    public void Parse_ThrowsException_BadHeader()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        string[] lines = ["Time,Dillon,Location,County,State,Lat,Lon,Comments"];
        var ows = new Mock<IOlieWebServices>();
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.Throws<Exception>(() => testable.Parse(effectiveDate, lines));
    }

    [Test]
    public void Parse_ThrowsException_MissingHeader()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        string[] lines = ["1805,UNK,3 NNE Carthage,Panola,TX,32.2,-94.32,Report of tree down on Co Rd 3022. (SHV)"];
        var ows = new Mock<IOlieWebServices>();
        var testable = new SpcSource(ows.Object);

        // Act, Assert
        Assert.Throws<Exception>(() => testable.Parse(effectiveDate, lines));
    }

    #endregion

    #region ParseTime

    [Test]
    public void ParseTime_SameDay_AfterNoon()
    {
        // Arrange
        const string time = "1845";
        var effectiveDate = new DateTime(2010, 7, 10);

        // Act
        var result = SpcSource.ParseTime(effectiveDate, time);

        // Assert
        Assert.That(result, Is.EqualTo(new DateTime(2010, 7, 10, 18, 45, 0)));
    }

    [Test]
    public void ParseTime_SameDay_BeforeNoon()
    {
        // Arrange
        const string time = "145";
        var effectiveDate = new DateTime(2010, 7, 10);

        // Act
        var result = SpcSource.ParseTime(effectiveDate, time);

        // Assert
        Assert.That(result, Is.EqualTo(new DateTime(2010, 7, 11, 1, 45, 0)));
    }

    #endregion
}