using OlievortexRed.Lib.Services;

namespace OlievortexRed.Lib.Processes;

public static class CommonProcess
{
    private const string IndexVideoPageId = "index";
    private const string Mp4Extension = ".mp4";

    public static readonly int[] Years =
    [
        2010, 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022,
        2023, 2024, 2025
    ];

    public static string CreateLocalTmpPath(string extension)
    {
        var tmpPath = Path.GetTempPath();
        var guid = Guid.NewGuid().ToString();

        return $"{tmpPath}{guid}{extension}";
    }

    public static void DeleteTempFiles(List<string> fileList, IOlieWebServices ows, params string[] additionalFiles)
    {
        var combined = fileList.Select(s => s).ToList();
        combined.AddRange(additionalFiles);

        foreach (var path in combined) ows.FileDelete(path);
    }
}