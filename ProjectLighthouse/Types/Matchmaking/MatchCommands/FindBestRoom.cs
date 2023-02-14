using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Types.Matchmaking.MatchCommands;

// Schema is the EXACT SAME as CreateRoom (but cant be a subclass here), so see comments there for details
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class FindBestRoom : IMatchCommand
{
    public int BuildVersion;
    public int HostMood;
    public int Language;
    public List<int> Location;

    public List<int> NAT;
    public int PassedNoJoinPoint;
    public RoomState RoomState;
    public string Search;
    public List<string> Players { get; set; }

    public List<string> Reservations { get; set; }
    public List<List<int>> Slots { get; set; }

    [JsonIgnore]
    public IEnumerable<int> FirstSlot => this.Slots[0];

    public RoomSlot RoomSlot
        => new()
        {
            SlotType = (SlotType)this.Slots[0][0],
            SlotId = this.Slots[0][1],
        };
}