using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Types.Matchmaking.MatchCommands;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class UpdatePlayersInRoom : IMatchCommand
{
    public List<string> Players { get; set; }
    public List<string> Reservations { get; set; }
}