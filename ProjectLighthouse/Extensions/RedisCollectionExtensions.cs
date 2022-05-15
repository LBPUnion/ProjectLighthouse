using System;
using System.Diagnostics.CodeAnalysis;
using Redis.OM.Searching;

namespace LBPUnion.ProjectLighthouse.Extensions;

[SuppressMessage("ReSharper", "LoopCanBePartlyConvertedToQuery")]
public static class RedisCollectionExtensions
{
    public static void DeleteAll<T>(this IRedisCollection<T> collection, Func<T, bool> predicate)
    {
        foreach (T item in collection)
        {
            if (!predicate.Invoke(item)) continue;

            collection.DeleteSync(item);
        }
    }

    public static void DeleteAll<T>(this IRedisCollection<T> collection)
    {
        foreach (T item in collection)
        {
            collection.DeleteSync(item);
        }
    }
}