#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Match;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Searching;

namespace LBPUnion.ProjectLighthouse;

public static class Redis
{
    private static readonly RedisConnectionProvider provider;

    static Redis()
    {
        provider = new RedisConnectionProvider(ServerConfiguration.Instance.RedisConnectionString);
    }

    private static bool initialized;
    public static async Task Initialize()
    {
        if (initialized) throw new InvalidOperationException("Redis has already been initialized.");

        IRedisConnection connection = getConnection();

        await connection.CreateIndexAsync(typeof(Room));
        await connection.CreateIndexAsync(typeof(UserFriendStore));

        initialized = true;
        Logger.LogSuccess("Initialized Redis.", LogArea.Redis);
    }

    private static IRedisConnection getConnection()
    {
        Logger.LogDebug("Getting a redis connection", LogArea.Redis);
        return provider.Connection;
    }

    public static IRedisCollection<Room> GetRooms() => provider.RedisCollection<Room>();
    
    private static IRedisCollection<UserFriendStore> userFriendStoreCollection => provider.RedisCollection<UserFriendStore>();

    public static UserFriendStore? GetUserFriendStore(int userId) =>
        userFriendStoreCollection.FirstOrDefault(s => s.UserId == userId);

    public static UserFriendStore CreateUserFriendStore(int userId)
    {
        UserFriendStore friendStore = new()
        {
            UserId = userId,
        };

        userFriendStoreCollection.Insert(friendStore);
        return friendStore;
    }

    public static void UpdateFriendStore(UserFriendStore friendStore)
    {
        userFriendStoreCollection.UpdateSync(friendStore);
    }
}