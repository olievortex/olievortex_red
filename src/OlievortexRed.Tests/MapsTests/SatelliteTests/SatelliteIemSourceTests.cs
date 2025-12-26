using Moq;
using OlievortexRed.Lib.Maps.Satellite;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Tests.MapsTests.SatelliteTests;

public class SatelliteIemSourceTests
{
    private readonly string _html = File.ReadAllText("./Resources/Iem/archive_data_2021_03_26_GIS_sat.html");

    #region IemList

    [Test]
    public async Task IemListAsync_List_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var url = Guid.NewGuid().ToString();
        var ows = new Mock<IOlieWebServices>();
        ows.Setup(s => s.ApiGetStringAsync(It.IsAny<string>(), ct))
            .ReturnsAsync(_html);
        var testable = new SatelliteIemSource(ows.Object);

        // Act
        var result = await testable.IemListAsync(url, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(288));
            Assert.That(result[0], Is.EqualTo("conus_goes_ir4km_0000.tif"));
        });
    }

    #endregion

    #region GetChannelFromKey

    [Test]
    public void GetChannelFromKey_14_InfraRed()
    {
        // Arrange
        const string value = "conus_goes_ir4km_0230.tif";
        const int expected = 14;
        var testable = new SatelliteIemSource(null!);

        // Act
        var result = testable.GetChannelFromKey(value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetChannelFromKey_2_Visible()
    {
        // Arrange
        const string value = "conus_goes_vis4km_0300.tif";
        const int expected = 2;
        var testable = new SatelliteIemSource(null!);

        // Act
        var result = testable.GetChannelFromKey(value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetChannelFromKey_10_WaterVapor()
    {
        // Arrange
        const string value = "conus_goes_wv4km_0300.tif";
        const int expected = 10;
        var testable = new SatelliteIemSource(null!);

        // Act
        var result = testable.GetChannelFromKey(value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetChannelFromKey_N1_Unknown()
    {
        // Arrange
        const string value = "dillon.tif";
        const int expected = -1;
        var testable = new SatelliteIemSource(null!);

        // Act
        var result = testable.GetChannelFromKey(value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetPath

    [Test]
    public void GetPrefix_Url_ValidParameters()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 18);
        const string expected = "https://mesonet.agron.iastate.edu/archive/data/2021/07/18/GIS/sat/";
        var testable = new SatelliteIemSource(null!);

        // Act
        var result = testable.GetPrefix(effectiveDate);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetScanTimeFromKey

    [Test]
    public void GetScanTimeFromKey_Value_ValidParameters()
    {
        // Arrange
        const string value = "conus_goes_vis4km_1830.tif";
        var effectiveDate = new DateTime(2021, 7, 18);
        var expected = new DateTime(2021, 7, 18, 18, 30, 0);
        var testable = new SatelliteIemSource(null!);

        // Act
        var result = testable.GetScanTimeFromKey(effectiveDate, value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion
}