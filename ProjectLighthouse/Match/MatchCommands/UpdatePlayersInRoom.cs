using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Match.MatchCommands;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class UpdatePlayersInRoom : IMatchCommand
{
    public List<string> Players { get; set; }
    public List<string> Reservations { get; set; }
}