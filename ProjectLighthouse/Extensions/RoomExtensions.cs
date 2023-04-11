#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class RoomExtensions
{
    public static List<UserEntity> GetPlayers(this Room room, DatabaseContext database)
    {
        List<UserEntity> players = new();
        foreach (int playerId in room.PlayerIds)
        {
            UserEntity? player = database.Users.FirstOrDefault(p => p.UserId == playerId);
            Debug.Assert(player != null, "RoomExtensions: player == null");

            players.Add(player);
        }

        return players;
    }
}