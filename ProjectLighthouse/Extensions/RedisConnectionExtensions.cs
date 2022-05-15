using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using Redis.OM;
using Redis.OM.Contracts;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class RedisConnectionExtensions
{
    public static async Task RecreateIndexAsync(this IRedisConnection connection, Type type)
    {
        Logger.LogDebug("Recreating index for " + type.Name, LogArea.Redis);
        
        // TODO: use `await connection.DropIndexAndAssociatedRecordsAsync(type);` here instead when that becomes a thing
        bool dropped = await connection.DropIndexAsync(type);
        Logger.LogDebug("Dropped index: " + dropped, LogArea.Redis);
        
        bool created = await connection.CreateIndexAsync(type);
        Logger.LogDebug("Created index: " + created, LogArea.Redis);
    }
}