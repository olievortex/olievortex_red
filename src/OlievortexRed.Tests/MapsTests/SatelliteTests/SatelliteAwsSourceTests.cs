using OlievortexRed.Lib.Maps.Satellite;

namespace OlievortexRed.Tests.MapsTests.SatelliteTests;

public class SatelliteAwsSourceTests
{
    #region GetBucketName

    [Test]
    public void GetBucketName_ReturnsBucket_ValidParameters()
    {
        // Arrange
        const int satellite = 16;
        var testable = new SatelliteAwsSource();

        // Act
        var result = testable.GetBucketName(satellite);

        // Assert
        Assert.That(result, Is.EqualTo("noaa-goes16"));
    }

    #endregion

    #region GetChannelFromAwsKey

    [Test]
    public void GetChannelFromAwsKey_ReturnsChannel_ValidKey()
    {
        // Arrange
        const string key =
            "https://noaa-goes16.s3.amazonaws.com/ABI-L1b-RadC/2021/044/10/OR_ABI-L1b-RadC-M6C07_G16_s20210441036054_e20210441038438_c20210441038494.nc";
        var testable = new SatelliteAwsSource();

        // Act
        var result = testable.GetChannelFromAwsKey(key);

        // Assert
        Assert.That(result, Is.EqualTo(7));
    }

    #endregion

    #region GetPrefix

    [Test]
    public void GetPrefix_ReturnsPrefix_ValidParameters()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 5, 18, 18, 0, 0);
        var testable = new SatelliteAwsSource();

        // Act
        var result = testable.GetPrefix(effectiveDate);

        // Assert
        Assert.That(result, Is.EqualTo("ABI-L1b-RadC/2021/138/18/"));
    }

    #endregion

    #region GetScanTime

    [Test]
    public void GetScanTime_ReturnsCreated_ValidFilename()
    {
        // Arrange
        const string filename = "OR_ABI-L1b-RadF-M3C02_G16_s20171671145342_e20171671156109_c20171671156144.nc";
        var expected = new DateTime(2017, 6, 16, 11, 45, 34, 200);
        var testable = new SatelliteAwsSource();

        // Act
        var result = testable.GetScanTime(filename);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion
}