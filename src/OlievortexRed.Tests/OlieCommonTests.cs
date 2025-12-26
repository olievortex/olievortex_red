using OlievortexRed.Lib;

namespace OlievortexRed.Tests;

public class OlieCommonTests
{
    #region ParseSpcEffectiveDate

    [Test]
    public void ParseEffectiveDate_ThrowsException_EmptyText()
    {
        Assert.Throws<ArgumentNullException>(() => OlieCommon.ParseSpcEffectiveDate(string.Empty));
    }

    #endregion

    #region TimeZoneToOffset

    [Test]
    public void TimeZoneToOffset_CorrectOffset_BatchA()
    {
        // Act, Arrange, Assert
        Assert.Multiple(() =>
        {
            Assert.That(OlieCommon.TimeZoneToOffset("utc"), Is.EqualTo(0));
            Assert.That(OlieCommon.TimeZoneToOffset("edt"), Is.EqualTo(-4));
            Assert.That(OlieCommon.TimeZoneToOffset("est"), Is.EqualTo(-5));
            Assert.That(OlieCommon.TimeZoneToOffset("cdt"), Is.EqualTo(-5));
            Assert.That(OlieCommon.TimeZoneToOffset("cst"), Is.EqualTo(-6));
            Assert.That(OlieCommon.TimeZoneToOffset("mdt"), Is.EqualTo(-6));
            Assert.That(OlieCommon.TimeZoneToOffset("mst"), Is.EqualTo(-7));
            Assert.That(OlieCommon.TimeZoneToOffset("pdt"), Is.EqualTo(-7));
            Assert.That(OlieCommon.TimeZoneToOffset("pst"), Is.EqualTo(-8));
            Assert.Throws<ApplicationException>(() => OlieCommon.TimeZoneToOffset("dillon"));
        });
    }

    #endregion
}