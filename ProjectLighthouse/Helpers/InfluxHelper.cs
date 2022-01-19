using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class InfluxHelper
{
    public static readonly InfluxDBClient Client = InfluxDBClientFactory.Create(ServerSettings.Instance.InfluxUrl, ServerSettings.Instance.InfluxToken);

    public static async void Log()
    {
        using WriteApi writeApi = Client.GetWriteApi();
        PointData point = PointData.Measurement("lighthouse")
            .Field("playerCount", await StatisticsHelper.RecentMatches())
            .Field("slotCount", await StatisticsHelper.SlotCount());

        writeApi.WritePoint(ServerSettings.Instance.InfluxBucket, ServerSettings.Instance.InfluxOrg, point);

        writeApi.Flush();
    }

    public static async Task StartLogging()
    {
        await Client.ReadyAsync();
        Logger.Log("InfluxDB is now ready.", LoggerLevelInflux.Instance);
        Thread t = new
        (
            delegate()
            {
                while (true)
                {
                    #pragma warning disable CS4014
                    Log();
                    #pragma warning restore CS4014
//                        Logger.Log("Logged.", LoggerLevelInflux.Instance);
                    Thread.Sleep(60000);
                }
            }
        );
        t.IsBackground = true;
        t.Name = "InfluxDB Logger";
        t.Start();
    }
}