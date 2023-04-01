#nullable enable
using System;
using System.Globalization;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;
using LBPUnion.ProjectLighthouse.Types.Users;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace LBPUnion.ProjectLighthouse.StorableLists;

public static class RedisDatabase
{
    private static readonly RedisConnectionProvider provider;

    static RedisDatabase()
    {
        ConnectionMultiplexer.SetFeatureFlag("preventthreadtheft", true);
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
                Logger.Error("Could not ping, ping returned " + pong,
                    LogArea.Redis);
                return;
            }

            await createIndexes(connection);
        }
        catch(Exception e)
        {
            Logger.Error("Could not initialize Redis:\n" + e, LogArea.Redis);
            return;
        }

        Initialized = true;
        Logger.Success("Initialized Redis.", LogArea.Redis);
    }

    public static async Task FlushAll()
    {
        IRedisConnection connection = getConnection();
        await connection.ExecuteAsync("FLUSHALL");

        await createIndexes(connection);
    }

    private static async Task createIndexes(IRedisConnection connection)
    {
        await connection.RecreateIndexAsync(typeof(Room));
        await connection.RecreateIndexAsync(typeof(UserFriendData));
    }

    private static IRedisConnection getConnection()
    {
        Logger.Debug("Getting a Redis connection", LogArea.Redis);
        return provider.Connection;
    }

    public static IRedisCollection<UserFriendData> UserFriendStoreCollection => provider.RedisCollection<UserFriendData>();

    internal static IRedisCollection<Room> GetRooms() => provider.RedisCollection<Room>();
}