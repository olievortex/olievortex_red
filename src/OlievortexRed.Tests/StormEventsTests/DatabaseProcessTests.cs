using Moq;
using OlievortexRed.Lib.Entities;
using OlievortexRed.Lib.StormEvents;
using OlievortexRed.Lib.StormEvents.Interfaces;
using OlievortexRed.Lib.StormEvents.Models;

namespace OlievortexRed.Tests.StormEventsTests;

public class DatabaseProcessTests
{
    #region DeactivateOldSummaries

    [Test]
    public async Task DeactivateOldSummariesAsync_AllSteps_ValidParameters()
    {
        // Arrange
        const string id = "2021-07-18";
        const int year = 2021;
        const string sourceFk = "20250401";
        var ct = CancellationToken.None;
        var business = new Mock<IDatabaseBusiness>();
        var testable = new DatabaseProcess(business.Object);
        var expected = new List<StormEventsDailySummaryEntity>
        {
            new() { SourceFk = sourceFk + sourceFk, IsCurrent = true }
        };
        business.Setup(s => s.GetSummariesForDayAsync(id, year, ct))
            .ReturnsAsync(expected);

        // Act
        var result = await testable.DeactivateOldSummariesAsync(id, year, sourceFk, ct);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region SourceDatabase

    [Test]
    public async Task SourceDatabaseAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var database = new Mock<IDatabaseBusiness>();
        var testable = new DatabaseProcess(database.Object);

        // Act
        await testable.SourceDatabasesAsync(null!, ct);

        // Assert
        database.Verify(v => v.DatabaseDownloadAsync(null!, It.IsAny<List<DatabaseFileModel>>(), ct),
            Times.Exactly(1));
    }

    #endregion

    #region Load

    [Test]
    public async Task LoadAsync_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var database = new Mock<IDatabaseBusiness>();
        var testable = new DatabaseProcess(database.Object);
        var inventory = new StormEventsDatabaseInventoryEntity();

        // Act
        await testable.LoadAsync(null!, inventory, ct);

        // Assert
        database.Verify(v => v.DatabaseLoadAsync(null!, inventory, ct),
            Times.Exactly(1));
    }

    #endregion

    #region GetAggregate

    [Test]
    public void GetAggregate_CompletesAllSteps_ValidParameters()
    {
        // Arrange
        var database = new Mock<IDatabaseBusiness>();
        var testable = new DatabaseProcess(database.Object);
        var models = new List<DailyDetailModel>();

        // Act
        var result = testable.GetAggregate(models);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    #endregion
}