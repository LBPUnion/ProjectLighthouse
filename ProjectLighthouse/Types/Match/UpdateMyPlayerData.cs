#nullable enable
namespace LBPUnion.ProjectLighthouse.Types.Match;

public class UpdateMyPlayerData : IMatchData
{
    public string Player { get; set; } = null!;

    public RoomState? RoomState { get; set; }
}