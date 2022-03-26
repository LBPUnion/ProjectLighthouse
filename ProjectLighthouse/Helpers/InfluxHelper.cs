using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class InfluxHelper
{
    public static readonly InfluxDBClient Client = InfluxDBClientFactory.Create(ServerSettings.Instance.InfluxUrl, ServerSettings.Instance.InfluxToken);

    private static readonly List<GameVersion> gameVersions = new()
    {
        GameVersion.LittleBigPlanet1,
        GameVersion.LittleBigPlanet2,
        GameVersion.LittleBigPlanet3,
        GameVersion.LittleBigPlanetVita,
        GameVersion.LittleBigPlanetPSP,
    };

    public static async void Log()
    {
        try
        {
            using WriteApi writeApi = Client.GetWriteApi();
            PointData point = PointData.Measurement("lighthouse")
                .Field("playerCount", await StatisticsHelper.RecentMatches())
                .Field("slotCount", await StatisticsHelper.SlotCount());

            foreach (GameVersion gameVersion in gameVersions)
            {
                PointData gamePoint = PointData.Measurement("lighthouse")
                    .Tag("game", gameVersion.ToString())
                    .Field("playerCountGame", await StatisticsHelper.RecentMatchesForGame(gameVersion));

                writeApi.WritePoint(ServerSettings.Instance.InfluxBucket, ServerSettings.Instance.InfluxOrg, gamePoint);
            }

            writeApi.WritePoint(ServerSettings.Instance.InfluxBucket, ServerSettings.Instance.InfluxOrg, point);

            writeApi.Flush();
        }
        catch(Exception e)
        {
            Logger.Log("Exception while logging: ", LoggerLevelInflux.Instance);

            foreach (string line in e.ToString().Split("\n")) Logger.Log(line, LoggerLevelInflux.Instance);
        }
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
                    try
                    {
                        Log();
                    }
                    catch(Exception e)
                    {
                        Logger.Log("Exception while running log thread: ", LoggerLevelInflux.Instance);

                        foreach (string line in e.ToString().Split("\n")) Logger.Log(line, LoggerLevelInflux.Instance);
                    }

                    Thread.Sleep(60000);
                }
            }
        );
        t.IsBackground = true;
        t.Name = "InfluxDB Logger";
        t.Start();
    }
}