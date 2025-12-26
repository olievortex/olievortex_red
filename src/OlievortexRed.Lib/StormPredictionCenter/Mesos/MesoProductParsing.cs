using System.Text.RegularExpressions;
using OlievortexRed.Lib.StormPredictionCenter.Interfaces;

namespace OlievortexRed.Lib.StormPredictionCenter.Mesos;

public partial class MesoProductParsing : IMesoProductParsing
{
    public string GetAreasAffected(string body)
    {
        var match = AreasAffectedRegex().Match(body);
        if (!match.Success)
            throw new ApplicationException("Unable get areas affected");

        var value = match.Groups[1].Value;
        var states = new List<string>();

        foreach (var item in OlieStates.FullToAbbr)
            if (value.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                states.Add(item.Value);
            else if (value.Contains(item.Value))
                states.Add(item.Value);

        var result = string.Join(", ", states.Order());

        return result;
    }

    public string GetBody(string html)
    {
        var match = BodyRegex().Match(html);
        if (!match.Success)
            throw new ApplicationException("Unable get body.");

        return match.Groups[1].Value.ReplaceLineEndings("\n");
    }

    public string GetConcerning(string body)
    {
        var match = ConcerningRegex().Match(body);
        if (!match.Success)
            throw new ApplicationException($"Unable to get concerning.");

        var result = match.Groups[1].Value.ReplaceLineEndings(" ");
        result = string.Join(" ",
            result.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        result = result.Replace("...", ". ");
        result = HtmlRegex().Replace(result, string.Empty).Trim();

        return result;
    }

    public DateTime GetEffectiveTime(string body)
    {
        var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length < 3)
            throw new ApplicationException($"Unable to get effective time.");

        return OlieCommon.ParseSpcEffectiveDate(lines[2]);
    }

    public string GetImageName(string html)
    {
        var match = MesoImageRegex().Match(html);

        if (!match.Success)
            throw new ApplicationException("Unable to get image name");

        return match.Value;
    }

    public string GetNarrative(string body)
    {
        var match = NarrativeRegex().Match(body);

        if (!match.Success)
            throw new ApplicationException("Unable to get narrative");

        var rawNarrative = match.Groups[1].Value;
        var trimNarrative = string.Join('\n', rawNarrative.Split('\n', StringSplitOptions.TrimEntries));
        trimNarrative = trimNarrative.Replace("...", ", ");
        var paragraphs =
            trimNarrative.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        paragraphs = paragraphs
            .Select(s =>
            {
                var parts = s.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var line = string.Join(" ", parts);

                return $"<p>{line}</p>";
            })
            .ToArray();

        return string.Join(null, paragraphs);
    }

    [GeneratedRegex(@"Areas affected\.\.\.([\w\W]*)Concerning", RegexOptions.IgnoreCase)]
    private static partial Regex AreasAffectedRegex();

    [GeneratedRegex(@"\<pre\>([\W\w]*)\<\/pre\>")]
    private static partial Regex BodyRegex();

    [GeneratedRegex(@"Concerning\.\.\.([\w\W]*)Valid", RegexOptions.IgnoreCase)]
    private static partial Regex ConcerningRegex();

    [GeneratedRegex(@"\<.*?\>")]
    private static partial Regex HtmlRegex();

    [GeneratedRegex(@"mcd\d{4}\.\w{3}")]
    private static partial Regex MesoImageRegex();

    [GeneratedRegex(@"Valid.*([\W\w]+)   \.\.", RegexOptions.IgnoreCase)]
    private static partial Regex NarrativeRegex();
}