using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Types.Match;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class UpdatePlayersInRoom : IMatchData
{
    public List<string> Players { get; set; }
    public List<string> Reservations { get; set; }
}