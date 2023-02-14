#nullable enable
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.StorableLists.Stores;

public static class RoomStore
{
    private static List<Room>? rooms;

    public static StorableList<Room> GetRooms()
    {
        if (RedisDatabase.Initialized)
        {
            return new RedisStorableList<Room>(RedisDatabase.GetRooms());
        }

        rooms ??= new List<Room>();
        return new NormalStorableList<Room>(rooms);
    }
}