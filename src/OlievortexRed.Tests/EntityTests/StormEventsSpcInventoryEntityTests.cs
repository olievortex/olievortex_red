using OlievortexRed.Lib.Entities;

namespace OlievortexRed.Tests.EntityTests;

public class StormEventsSpcInventoryEntityTests
{
    [Test]
    public void FromValues_CreatesEntity_ValidParameters()
    {
        // Arrange
        var effectiveDate = new DateTime(2021, 7, 10);
        const string body = "a\nb";
        const string etag = "c";

        // Act
        var result = StormEventsSpcInventoryEntity.FromValues(effectiveDate, body, etag);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo("c"));
            Assert.That(result.EffectiveDate, Is.EqualTo("2021-07-10"));
            Assert.That(result.IsDailySummaryComplete, Is.False);
            Assert.That(result.IsTornadoDay, Is.False);
            Assert.That(result.Timestamp, Is.Not.EqualTo(DateTime.MinValue));
            Assert.That(result.Rows, Has.Length.EqualTo(2));
            Assert.That(result.Rows[0], Is.EqualTo("a"));
            Assert.That(result.Rows[1], Is.EqualTo("b"));
            Assert.That(result.IsDailySummaryComplete, Is.False);
        });
    }

    [Test]
    public void DecodeEffectiveDate_Decodes_ValidEffectiveDate()
    {
        // Arrange
        var entity = new StormEventsSpcInventoryEntity
        {
            EffectiveDate = "2010-07-25"
        };
        var expected = new DateTime(2010, 7, 25);

        // Act
        var result = entity.DecodeEffectiveDate();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}