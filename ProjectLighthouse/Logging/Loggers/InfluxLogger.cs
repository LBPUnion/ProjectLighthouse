using InfluxDB.Client;
using InfluxDB.Client.Writes;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Logging.Loggers;

public class InfluxLogger : ILogger
{
    public void Log(LogLine line)
    {
        string channel = string.IsNullOrEmpty(line.Area) ? "" : $"[{line.Area}] ";

        string level = $"{$"{channel} {line}".TrimEnd()}";
        string content = line.Message;

        using WriteApi writeApi = InfluxHelper.Client.GetWriteApi();

        PointData point = PointData.Measurement("lighthouseLog").Field("level", level).Field("content", content);

        writeApi.WritePoint(point, ServerConfiguration.Instance.InfluxDB.Bucket, ServerConfiguration.Instance.InfluxDB.Organization);
    }
}