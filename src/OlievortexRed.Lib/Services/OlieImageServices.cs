using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Diagnostics.CodeAnalysis;

namespace OlievortexRed.Lib.Services;

[ExcludeFromCodeCoverage]
public class OlieImageServices : IOlieImageServices
{
    private readonly IResampler _sampler = KnownResamplers.Lanczos2;

    public async Task<byte[]> ResizeAsync(byte[] bitmap, Point finalSize, CancellationToken ct)
    {
        using var image = Image.Load(bitmap);
        image.Mutate(x => x.Resize(finalSize.X, finalSize.Y, _sampler));
        using var ms = new MemoryStream(512000);
        await image.SaveAsGifAsync(ms, ct);
        return ms.ToArray();
    }
}