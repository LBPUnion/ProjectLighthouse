#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class RoomExtensions
{
    public static List<User> GetPlayers(this Room room, Database database)
    {
        List<User> players = new();
        foreach (int playerId in room.PlayerIds)
        {
            User? player = database.Users.FirstOrDefault(p => p.UserId == playerId);
            Debug.Assert(player != null);

            players.Add(player);
        }

        return players;
    }
}