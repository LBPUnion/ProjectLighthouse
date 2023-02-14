#nullable enable
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Types.Matchmaking.MatchCommands;

public class UpdateMyPlayerData : IMatchCommand
{
    public string Player { get; set; } = null!;

    public RoomState? RoomState { get; set; }
}