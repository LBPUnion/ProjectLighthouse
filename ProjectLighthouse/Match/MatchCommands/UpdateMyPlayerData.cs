#nullable enable
using LBPUnion.ProjectLighthouse.Match.Rooms;

namespace LBPUnion.ProjectLighthouse.Match.MatchCommands;

public class UpdateMyPlayerData : IMatchCommand
{
    public string Player { get; set; } = null!;

    public RoomState? RoomState { get; set; }
}