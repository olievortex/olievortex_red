namespace OlievortexRed.Lib.Maps.Interfaces;

public interface ISatelliteAwsSource
{
    string GetBucketName(int satellite);

    int GetChannelFromAwsKey(string key);

    string GetPrefix(DateTime effectiveHour);

    DateTime GetScanTime(string filename);
}