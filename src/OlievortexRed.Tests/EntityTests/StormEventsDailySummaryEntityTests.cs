using OlievortexRed.Lib.Entities;

namespace OlievortexRed.Tests.EntityTests;

public class StormEventsDailySummaryEntityTests
{
    [Test]
    public void ToString_EffectiveDate_ValidParameters()
    {
        // Arrange
        var testable = new StormEventsDailySummaryEntity
        {
            Id = "2021-07-18"
        };

        // Act
        var result = testable.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("2021-07-18"));
    }
}