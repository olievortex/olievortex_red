using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Radar;
using OlievortexRed.Lib.Radar.Interfaces;

namespace OlievortexRed.Tests.RadarTests;

public class RadarBusinessTests
{
    #region DownloadInventoryForClosestRadar

    [Test]
    public async Task DownloadInventoryForClosestRadarAsync_AllSteps_ValidParameters()
    {
        // Arrange
        var effectiveTime = new DateTime(2021, 7, 21, 18, 25, 0);
        var ct = CancellationToken.None;
        const double latitude = 45;
        const double longitude = -93;
        var expected = new RadarSiteEntity();
        var radarSites = new List<RadarSiteEntity>();
        var source = new Mock<IRadarSource>();
        source.Setup(s => s.FindClosestRadar(radarSites, latitude, longitude))
            .Returns(expected);
        var testable = new RadarBusiness(source.Object);
        var cache = new List<RadarInventoryEntity>();

        // Act
        var result =
            await testable.DownloadInventoryForClosestRadarAsync(radarSites, cache, effectiveTime, latitude, longitude,
                null!, ct);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region PopulateRadarSitesFromCsv

    [Test]
    public async Task PopulateCosmosFromCsvAsync_AddsRecord_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var entity = new RadarSiteEntity();
        const string value =
            """
            NCDCID   ICAO WBAN  NAME                           COUNTRY              ST COUNTY                         LAT       LON        ELEV   UTC   STNTYPE                                            
            -------- ---- ----- ------------------------------ -------------------- -- ------------------------------ --------- ---------- ------ ----- -------------------------------------------------- 
            30001794 KABR 14929 ABERDEEN                       UNITED STATES           BROWN                          45.455833 -98.413333 1383   -6    NEXRAD                                             
            30001795 KABX 03019 ALBUQUERQUE                    UNITED STATES        NM BERNALILLO                     35.149722 -106.82388 5951   -7    NEXRAD                                             

            """;
        var source = new Mock<IRadarSource>();
        source.Setup(s => s.CreateRadarSiteAsync(It.IsAny<RadarSiteEntity>(), ct))
            .Callback((RadarSiteEntity e, CancellationToken _) => entity = e);
        var testable = new RadarBusiness(source.Object);

        // Act
        await testable.PopulateRadarSitesFromCsvAsync(value, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo("KABX"));
            Assert.That(entity.Name, Is.EqualTo("ALBUQUERQUE"));
            Assert.That(entity.State, Is.EqualTo("NM"));
            Assert.That(entity.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(entity.Latitude, Is.EqualTo(35.149722));
            Assert.That(entity.Longitude, Is.EqualTo(-106.82388));
        });
    }

    #endregion
}