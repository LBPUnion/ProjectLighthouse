using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class InfluxHelper
{
    public static readonly InfluxDBClient Client = new(url: ServerConfiguration.Instance.InfluxDB.Url,
        token: ServerConfiguration.Instance.InfluxDB.Token);

    private static readonly List<GameVersion> gameVersions = new()
    {
        GameVersion.LittleBigPlanet1,
        GameVersion.LittleBigPlanet2,
        GameVersion.LittleBigPlanet3,
        GameVersion.LittleBigPlanetVita,
        GameVersion.LittleBigPlanetPSP,
    };

    private static async void Log()
    {
        try
        {
            await using Database database = new();
            using WriteApi writeApi = Client.GetWriteApi();
            PointData point = PointData.Measurement("lighthouse")
                .Field("playerCount", await StatisticsHelper.RecentMatches(database))
                .Field("slotCount", await StatisticsHelper.SlotCount(database))
                .Field("userCount", await StatisticsHelper.UserCount(database))
                .Field("photoCount", await StatisticsHelper.PhotoCount(database));

            foreach (GameVersion gameVersion in gameVersions)
            {
                PointData gamePoint = PointData.Measurement("lighthouse")
                    .Tag("game", gameVersion.ToString())
                    .Field("playerCountGame", await StatisticsHelper.RecentMatchesForGame(database, gameVersion));

                writeApi.WritePoint(gamePoint, ServerConfiguration.Instance.InfluxDB.Bucket, ServerConfiguration.Instance.InfluxDB.Organization);
            }

            writeApi.WritePoint(point, ServerConfiguration.Instance.InfluxDB.Bucket, ServerConfiguration.Instance.InfluxDB.Organization);

            writeApi.Flush();
        }
        catch(Exception e)
        {
            Logger.Error("Exception while logging: ", LogArea.InfluxDB);
            Logger.Error(e.ToDetailedException(), LogArea.InfluxDB);
        }
    }

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static async Task StartLogging()
    {
        await Client.ReadyAsync();
        Logger.Success("InfluxDB is now ready.", LogArea.InfluxDB);
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
                        Logger.Error("Exception while running log thread: ", LogArea.InfluxDB);
                        Logger.Error(e.ToDetailedException(), LogArea.InfluxDB);
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