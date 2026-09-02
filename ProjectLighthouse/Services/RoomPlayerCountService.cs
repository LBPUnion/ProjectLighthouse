#nullable enable
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Services;

public class RoomPlayerCountService
{
    private readonly Dictionary<int, int> playerCounts = RoomHelper.GetUserLevelPlayerCounts();

    public int GetPlayerCount(int slotId)
    {
        return this.playerCounts.TryGetValue(slotId, out int playerCount)
            ? playerCount
            : 0;
    }
}
