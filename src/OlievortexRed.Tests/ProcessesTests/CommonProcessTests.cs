using Moq;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.Services;

namespace OlievortexRed.Tests.ProcessesTests;

public class CommonProcessTests
{
    [Test]
    public void CreateLocalTmpPath_ReturnsPath_Always()
    {
        // Arrange
        const string extension = ".olie";

        // Act
        var result = CommonProcess.CreateLocalTmpPath(extension);

        // Assert
        Assert.That(result, Does.EndWith(".olie"));
    }

    [Test]
    public void DeleteTempFiles_DeletesFiles_WithParams()
    {
        // Arrange
        var ows = new Mock<IOlieWebServices>();
        var files = new List<string> { "a.tmp", "b.tmp" };
        const string file = "c.tmp";

        // Act
        CommonProcess.DeleteTempFiles(files, ows.Object, file);

        // Assert
        ows.Verify(v => v.FileDelete(It.IsAny<string>()), Times.Exactly(3));
    }
}