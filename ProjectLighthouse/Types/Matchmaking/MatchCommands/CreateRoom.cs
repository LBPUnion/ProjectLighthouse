using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

namespace LBPUnion.ProjectLighthouse.Types.Matchmaking.MatchCommands;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
public class CreateRoom : IMatchCommand
{
    public int BuildVersion;
    public int HostMood;
    public int Language;
    public List<int> Location;

    public List<int> NAT;
    public int PassedNoJoinPoint;
    public RoomState RoomState;

    public string Search;

    //[CreateRoom,["Players":["LumaLivy"],"Reservations":["0"],"NAT":[2],"Slots":[[1,3]],"RoomState":0,"HostMood":1,"PassedNoJoinPoint":0,"Location":[0x7f000001],"Language":1,"BuildVersion":289,"Search":""]]
    public List<string> Players { get; set; }

    public List<string> Reservations { get; set; }

    //           v slot type, 1 = 2974
    // "Slots":[[5,0]]
    //             ^ slot id
    // no idea why this is an array, but we'll work with it i suppose
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