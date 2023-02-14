using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Redis.OM;
using Redis.OM.Contracts;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class RedisConnectionExtensions
{
    public static async Task RecreateIndexAsync(this IRedisConnection connection, Type type)
    {
        Logger.Debug("Recreating index for " + type.Name, LogArea.Redis);
        
        // TODO: use `await connection.DropIndexAndAssociatedRecordsAsync(type);` here instead when that becomes a thing
        bool dropped = await connection.DropIndexAsync(type);
        Logger.Debug("Dropped index: " + dropped, LogArea.Redis);
        
        bool created = await connection.CreateIndexAsync(type);
        Logger.Debug("Created index: " + created, LogArea.Redis);
    }
}