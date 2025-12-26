using OlievortexRed.Lib.Maps.Interfaces;

namespace OlievortexRed.Lib.Maps.Satellite;

public class SatelliteAwsSource : ISatelliteAwsSource
{
    public string GetBucketName(int satellite)
    {
        return $"noaa-goes{satellite}";
    }

    public int GetChannelFromAwsKey(string key)
    {
        var fileName = key.Split('/')[^1];
        return int.Parse(fileName.Split('_')[1][^2..]);
    }

    public string GetPrefix(DateTime effectiveHour)
    {
        var dateTimeFolder = $"{effectiveHour:yyyy}/{effectiveHour.DayOfYear:000}/{effectiveHour:HH}/";
        return $"ABI-L1b-RadC/{dateTimeFolder}";
    }

    public DateTime GetScanTime(string filename)
    {
        // OR_ABI-L1b-RadF-M3C02_G16_s20171671145342_e20171671156109_c20171671156144.nc
        var parts = filename.Split('_');
        var created = parts[3][1..];

        var year = int.Parse(created[..4]);
        var dayNumber = int.Parse(created[4..7]);
        var hour = int.Parse(created[7..9]);
        var minute = int.Parse(created[9..11]);
        var second = int.Parse(created[11..13]);
        var millisecond = (created[13] - '0') * 100;

        var result = new DateTime(year, 1, 1, hour, minute, second, millisecond, DateTimeKind.Utc)
            .AddDays(dayNumber - 1);

        return result;
    }
}