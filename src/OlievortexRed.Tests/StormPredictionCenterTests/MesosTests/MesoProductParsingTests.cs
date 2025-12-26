using OlievortexRed.Lib.StormPredictionCenter.Mesos;

namespace OlievortexRed.Tests.StormPredictionCenterTests.MesosTests;

public class MesoProductParsingTests
{
    private readonly string _html = File.ReadAllText("./Resources/Mesos/meso0170.html");

    #region GetAreasAffected

    [Test]
    public void GetAreasAffected_ThrowsException_BadBody()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act, Assert
        Assert.Throws<ApplicationException>(() => testable.GetAreasAffected(string.Empty));
    }

    [Test]
    public void GetAreasAffected_AreasAffected_ValidBody()
    {
        // Arrange
        var testable = new MesoProductParsing();
        var body = testable.GetBody(_html);

        // Act
        var result = testable.GetAreasAffected(body);

        // Assert
        Assert.That(result, Is.EqualTo("AR, IA, KS, MN, MO, OK"));
    }

    #endregion

    #region GetAffectiveTime

    [Test]
    public void GetEffectiveTime_ThrowsException_BadBody()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act, Assert
        Assert.Throws<ApplicationException>(() => testable.GetEffectiveTime(string.Empty));
    }

    [Test]
    public void GetEffectiveTime_EffectiveTime_ValidBody()
    {
        // Arrange
        var testable = new MesoProductParsing();
        var body = testable.GetBody(_html);
        var expected = new DateTime(2025, 3, 14, 18, 1, 0);

        // Act
        var result = testable.GetEffectiveTime(body);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion

    #region GetImageName

    [Test]
    public void GetImageName_ThrowsException_BadHtml()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act, Assert
        Assert.Throws<ApplicationException>(() => testable.GetImageName(string.Empty));
    }

    [Test]
    public void GetImageName_ImageName_ValidHtml()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act
        var result = testable.GetImageName(_html);

        // Assert
        Assert.That(result, Is.EqualTo("mcd0170.png"));
    }

    #endregion

    #region GetConcerning

    [Test]
    public void GetConcerning_ThrowsException_BadBody()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act, Assert
        Assert.Throws<ApplicationException>(() => testable.GetConcerning(string.Empty));
    }

    [Test]
    public void GetConcerning_Concerning_ValidBody()
    {
        // Arrange
        var testable = new MesoProductParsing();
        var body = testable.GetBody(_html);

        // Act
        var result = testable.GetConcerning(body);

        // Assert
        Assert.That(result, Is.EqualTo("Severe potential. Watch likely"));
    }

    #endregion

    #region GetBody

    [Test]
    public void GetBody_ThrowsException_BadHtml()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act, Assert
        Assert.Throws<ApplicationException>(() => testable.GetBody(string.Empty));
    }

    [Test]
    public void GetBody_BodyName_ValidHtml()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act
        var result = testable.GetBody(_html);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Does.StartWith("\n\n   Mesoscale Discussion 0170"));
            Assert.That(result, Does.EndWith("40079546 40859479 \n\n"));
        });
    }

    #endregion

    #region GetNarrative

    [Test]
    public void GetNarrative_ThrowsException_BadHtml()
    {
        // Arrange
        var testable = new MesoProductParsing();

        // Act, Assert
        Assert.Throws<ApplicationException>(() => testable.GetNarrative(string.Empty));
    }

    [Test]
    public void GetNarrative_Narrative_ValidBody()
    {
        // Arrange
        var testable = new MesoProductParsing();
        var body = testable.GetBody(_html);

        // Act
        var result = testable.GetNarrative(body);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Does.StartWith("<p>Probability of Watch Issuance, 95 percent</p>"));
            Assert.That(result, Does.EndWith("<p>..Kerr/Gleason.. 03/14/2025</p>"));
        });
    }

    #endregion
}