using Moq;
using OlievortexRed.Lib.Processes;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;

namespace OlievortexRed.Tests.ProcessesTests;

public class SpcMesosProcessTests
{
    #region Run

    [Test]
    public async Task RunAsync_CompletesSteps_ValidParameters()
    {
        // Arrange
        var ct = CancellationToken.None;
        var process = new Mock<IMesoProductProcess>();
        process.SetupSequence(s => s.DownloadAsync(It.IsAny<int>(), It.IsAny<int>(), null!, ct))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        var testable = new SpcMesosProcess(process.Object);

        // Act
        await testable.RunAsync(null!, true, false, ct);

        // Assert
        process.Verify(v => v.DownloadAsync(It.IsAny<int>(), It.IsAny<int>(), null!, ct),
            Times.Exactly(2));
    }

    #endregion

    #region DoSomething

    [Test]
    public async Task DoSomethingAsync_CompletesSteps_UpdateOnly()
    {
        // Arrange
        const int year = 2023;
        const int index = 734;
        var ct = CancellationToken.None;
        var process = new Mock<IMesoProductProcess>();
        process.SetupSequence(s => s.DownloadAsync(It.IsAny<int>(), It.IsAny<int>(), null!, ct))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        var testable = new SpcMesosProcess(process.Object);

        // Act
        await testable.DoSomethingAsync(year, index, true, null!, ct);

        // Assert
        process.Verify(v => v.UpdateAsync(year, index, ct),
            Times.Exactly(1));
    }

    #endregion
}