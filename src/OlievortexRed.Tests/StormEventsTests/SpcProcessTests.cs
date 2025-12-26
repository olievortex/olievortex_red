using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.StormEventsTests;

public class SpcProcessTests
{
    #region GetInventoryByYear

    [Test]
    public async Task GetInventoryByYearAsync_FetchesInventory_ValidYear()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int year = 2024;
        var spc = new Mock<ISpcBusiness>();
        var testable = new SpcProcess(spc.Object);
        var expected = new List<StormEventsSpcInventoryEntity>();
        spc.Setup(s => s.GetInventoryByYearAsync(year, ct))
            .ReturnsAsync(expected);

        // Test
        var (_, __, items) = await testable.GetInventoryByYearAsync(year, ct);

        // Assert
        Assert.That(items, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetInventoryByYearAsync_NoInventory_OldYear()
    {
        // Arrange
        var ct = CancellationToken.None;
        const int year = 2018;
        var spc = new Mock<ISpcBusiness>();
        var testable = new SpcProcess(spc.Object);
        var expected = new List<StormEventsSpcInventoryEntity>();
        spc.Setup(s => s.GetInventoryByYearAsync(year, ct))
            .ReturnsAsync(expected);

        // Test
        var (_, __, items) = await testable.GetInventoryByYearAsync(year, ct);

        // Assert
        Assert.That(items, Is.Empty);
    }

    #endregion

    #region ProcessEvents

    [Test]
    public async Task ProcessEvents_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var business = new Mock<ISpcBusiness>();
        var testable = new SpcProcess(business.Object);
        var inventory = new StormEventsSpcInventoryEntity();
        var events = new List<DailyDetailModel>();

        // Act
        await testable.ProcessEvents(events, inventory, ct);

        // Assert
        business.Verify(
            v => v.AddDailyDetailAsync(It.IsAny<List<DailyDetailModel>>(),
                It.IsAny<StormEventsSpcInventoryEntity>(), ct),
            Times.Exactly(1));
    }

    #endregion

    #region ShouldSkip

    [Test]
    public void ShouldSkip_True_AlreadyComplete()
    {
        // Arrange
        var spc = new Mock<ISpcBusiness>();
        var testable = new SpcProcess(spc.Object);
        var inventory = new StormEventsSpcInventoryEntity
        {
            IsDailySummaryComplete = true,
            IsDailyDetailComplete = true
        };

        // Act
        var result = testable.ShouldSkip(inventory);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region SourceInventory

    [Test]
    public async Task SourceInventoryAsync_DownloadsNew_Missing()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2010, 5, 4);
        var inventoryList = new List<StormEventsSpcInventoryEntity>();
        var entity = new StormEventsSpcInventoryEntity();
        var spc = new Mock<ISpcBusiness>();
        spc.Setup(s => s.GetLatest(effectiveDate, inventoryList))
            .Returns((StormEventsSpcInventoryEntity?)null);
        spc.Setup(s => s.DownloadNewAsync(effectiveDate, ct))
            .ReturnsAsync(entity);
        var testable = new SpcProcess(spc.Object);

        // Act
        var result = await testable.SourceInventoryAsync(effectiveDate, inventoryList, ct);

        // Assert
        Assert.That(result, Is.EqualTo(entity));
    }

    [Test]
    public async Task SourceInventoryAsync_ChecksForUpdates_ExistingEntry()
    {
        // Arrange
        var ct = CancellationToken.None;
        var effectiveDate = new DateTime(2010, 5, 4);
        var inventoryList = new List<StormEventsSpcInventoryEntity>();
        var inventory = new StormEventsSpcInventoryEntity();
        var spc = new Mock<ISpcBusiness>();
        spc.Setup(s => s.DownloadUpdateAsync(inventory, ct))
            .ReturnsAsync(inventory);
        spc.Setup(s => s.GetLatest(effectiveDate, inventoryList))
            .Returns(inventory);
        var testable = new SpcProcess(spc.Object);

        // Act
        var result = await testable.SourceInventoryAsync(effectiveDate, inventoryList, ct);

        // Assert
        Assert.That(result, Is.EqualTo(inventory));
    }

    #endregion
}