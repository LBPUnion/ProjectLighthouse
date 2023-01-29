#nullable enable
using LBPUnion.ProjectLighthouse.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Matchmaking.MatchCommands;

public class UpdateMyPlayerData : IMatchCommand
{
    public string Player { get; set; } = null!;

    public RoomState? RoomState { get; set; }
}