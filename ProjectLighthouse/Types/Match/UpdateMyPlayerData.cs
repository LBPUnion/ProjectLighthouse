#nullable enable
namespace LBPUnion.ProjectLighthouse.Types.Match
{
    public class UpdateMyPlayerData : IMatchData
    {
        public string Player { get; set; }

        public RoomState? RoomState { get; set; }
    }
}