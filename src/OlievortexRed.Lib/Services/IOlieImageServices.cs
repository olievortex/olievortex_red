using SixLabors.ImageSharp;

namespace OlievortexRed.Lib.Services;

public interface IOlieImageServices
{
    Task<byte[]> ResizeAsync(byte[] bitmap, Point finalSize, CancellationToken ct);
}