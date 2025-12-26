using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.Radar;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Tests.RadarTests;

public class RadarSourceTests
{
    #region AddRadarInventory

    [Test]
    public async Task AddRadarInventoryAsync_AllSteps_ValidParameters()
    {
        // Arrange
        const string radarId = "KMSP";
        var effectiveTime = new DateTime(2021, 7, 18, 21, 45, 0);
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var ows = new Mock<IOlieWebServices>();
        var testable = new RadarSource(cosmos.Object, ows.Object);
        var cache = new List<RadarInventoryEntity>();
        var radar = new RadarSiteEntity
        {
            Id = radarId
        };
        var items = new List<string> { "a", "b", "c" };
        ows.Setup(s => s.AwsListAsync(RadarSource.LevelIiBucket, It.IsAny<string>(), null!, ct))
            .ReturnsAsync(items);

        // Act
        await testable.AddRadarInventoryAsync(cache, radar, effectiveTime, null!, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(cache, Has.Count.EqualTo(1));
            Assert.That(cache[0].EffectiveDate, Is.EqualTo("2021-07-18"));
            Assert.That(cache[0].Timestamp, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(cache[0].BucketName, Is.EqualTo(RadarSource.LevelIiBucket));
            Assert.That(cache[0].Id, Is.EqualTo(radar.Id));
            Assert.That(cache[0].FileList, Is.EqualTo(items));
        });
    }

    #endregion

    #region CreateRadarSite

    [Test]
    public async Task CreateRadarSiteAsync_AllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new RadarSource(cosmos.Object, null!);
        var entity = new RadarSiteEntity();

        // Act
        await testable.CreateRadarSiteAsync(entity, ct);

        // Assert
        cosmos.Verify(v => v.RadarSiteCreateAsync(entity, ct), Times.Exactly(1));
    }

    #endregion

    #region FindClosestRadar

    [Test]
    public void FindClosestRadar_Closest_ValidParameters()
    {
        // Arrange
        var sites = new List<RadarSiteEntity>
        {
            new() { Id = "a", Latitude = 43, Longitude = -95 },
            new() { Id = "b", Latitude = 44, Longitude = -92 },
            new() { Id = "c", Latitude = 47, Longitude = -91 }
        };
        var testable = new RadarSource(null!, null!);

        // Act
        var result = testable.FindClosestRadar(sites, 45, -93);

        // Assert
        Assert.That(result.Id, Is.EqualTo("b"));
    }

    #endregion

    #region GetPrimaryRadarSites

    [Test]
    public async Task GetPrimaryRadarSitesAsync_AllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new List<RadarSiteEntity> { new() { Id = "KILO" } };
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new RadarSource(cosmos.Object, null!);
        cosmos.Setup(s => s.RadarSiteAllAsync(ct)).ReturnsAsync(expected);

        // Act
        var result = await testable.GetPrimaryRadarSitesAsync(ct);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetRadarInventory

    [Test]
    public async Task GetRadarInventory_ShortCircuit_CacheHit()
    {
        // Arrange
        const string radarId = "KMSP";
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new RadarSource(cosmos.Object, null!);
        var ct = CancellationToken.None;
        var effectiveTime = new DateTime(2021, 7, 18, 21, 15, 0);
        var radar = new RadarSiteEntity
        {
            Id = radarId
        };
        var expected = new RadarInventoryEntity
        {
            Id = radarId,
            BucketName = RadarSource.LevelIiBucket,
            EffectiveDate = "2021-07-18"
        };
        var cache = new List<RadarInventoryEntity> { expected };

        // Act
        var result = await testable.GetRadarInventoryAsync(cache, radar, effectiveTime, ct);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetRadarInventory_Null_NoRecord()
    {
        // Arrange
        const string radarId = "KMSP";
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new RadarSource(cosmos.Object, null!);
        var ct = CancellationToken.None;
        var effectiveTime = new DateTime(2021, 7, 18, 21, 15, 0);
        var radar = new RadarSiteEntity
        {
            Id = radarId
        };
        var cache = new List<RadarInventoryEntity>();

        // Act
        var result = await testable.GetRadarInventoryAsync(cache, radar, effectiveTime, ct);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRadarInventory_UpdatesCache_RecordFound()
    {
        // Arrange
        const string radarId = "KMSP";
        const string effectiveDate = "2021-07-18";
        var cosmos = new Mock<ICosmosRepository>();
        var testable = new RadarSource(cosmos.Object, null!);
        var ct = CancellationToken.None;
        var effectiveTime = new DateTime(2021, 7, 18, 21, 15, 0);
        var radar = new RadarSiteEntity
        {
            Id = radarId
        };
        var expected = new RadarInventoryEntity
        {
            Id = radarId,
            BucketName = RadarSource.LevelIiBucket,
            EffectiveDate = effectiveDate,
            FileList = [string.Empty],
            Timestamp = DateTime.UtcNow
        };
        cosmos.Setup(s => s.RadarInventoryGetAsync(radarId, effectiveDate, RadarSource.LevelIiBucket, ct))
            .ReturnsAsync(expected);
        var cache = new List<RadarInventoryEntity>();

        // Act
        var result = await testable.GetRadarInventoryAsync(cache, radar, effectiveTime, ct);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(cache, Has.Count.EqualTo(1));
            Assert.That(result?.FileList, Has.Count.EqualTo(1));
            Assert.That(result?.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
        });
    }

    #endregion
}