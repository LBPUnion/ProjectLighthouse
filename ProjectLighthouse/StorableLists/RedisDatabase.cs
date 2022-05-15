#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Match.Rooms;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Types;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;

namespace LBPUnion.ProjectLighthouse.StorableLists;

public static class RedisDatabase
{
    private static readonly RedisConnectionProvider provider;

    static RedisDatabase()
    {
        provider = new RedisConnectionProvider(ServerConfiguration.Instance.RedisConnectionString);
    }

    public static bool Initialized { get; private set; }
    public static async Task Initialize()
    {
        if (Initialized) throw new InvalidOperationException("Redis has already been initialized.");

        try
        {
            IRedisConnection connection = getConnection();

            string pong = (await connection.ExecuteAsync("PING")).ToString(CultureInfo.InvariantCulture);
            if (pong != "PONG")
            {
                Logger.LogError("Could not ping, ping returned " + pong,
                    LogArea.Redis);
                return;
            }

            await connection.RecreateIndexAsync(typeof(Room));
            await connection.RecreateIndexAsync(typeof(UserFriendData));
        }
        catch(Exception e)
        {
            Logger.LogError("Could not initialize Redis:\n" + e, LogArea.Redis);
            return;
        }

        Initialized = true;
        Logger.LogSuccess("Initialized Redis.", LogArea.Redis);
    }

    private static IRedisConnection getConnection()
    {
        Logger.LogDebug("Getting a Redis connection", LogArea.Redis);
        return provider.Connection;
    }

    public static IRedisCollection<UserFriendData> UserFriendStoreCollection => provider.RedisCollection<UserFriendData>();

    internal static IRedisCollection<Room> GetRooms() => provider.RedisCollection<Room>();
}