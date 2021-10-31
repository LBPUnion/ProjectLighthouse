using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    public class UpdatePlayersInRoom : IMatchData
    {
        public List<string> Players;
        public List<string> Reservations;
    }
}