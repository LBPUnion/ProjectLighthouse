using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public class PlayerCountResponse
{
    public Dictionary<GameVersion, int> PlayerCounts { get; set; } = new();
}