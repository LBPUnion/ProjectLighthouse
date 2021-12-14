using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Logging
{
    public class InfluxLogger : LoggerBase
    {
        public override bool AllowMultiple => false;

        public override void Send(LoggerLine line)
        {
            string channel = string.IsNullOrEmpty(line.LoggerLevel.Channel) ? "" : $"[{line.LoggerLevel.Channel}] ";

            string level = $"{$"{line.LoggerLevel.Name} {channel}".TrimEnd()}";
            string content = line.LineData;

            using WriteApi writeApi = InfluxHelper.Client.GetWriteApi();

            PointData point = PointData.Measurement("lighthouseLog").Field("level", level).Field("content", content);

            writeApi.WritePoint(ServerSettings.Instance.InfluxBucket, ServerSettings.Instance.InfluxOrg, point);
        }
    }
}