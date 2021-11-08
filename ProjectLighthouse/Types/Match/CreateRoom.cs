using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    public class CreateRoom : IMatchData
    {
        //[CreateRoom,["Players":["LumaLivy"],"Reservations":["0"],"NAT":[2],"Slots":[[1,3]],"RoomState":0,"HostMood":1,"PassedNoJoinPoint":0,"Location":[0x7f000001],"Language":1,"BuildVersion":289,"Search":""]]
        public List<string> Players { get; set; }
        public List<string> Reservations { get; set; }
        //           v slot type, 1 = 2974
        // "Slots":[[5,0]]
        //             ^ slot id
        // no idea why this is an array, but we'll work with it i suppose
        public List<List<int>> Slots { get; set; }
        [JsonIgnore]
        public List<int> FirstSlot => this.Slots[0];
        public List<int> NAT;
        public int RoomState;
        public int HostMood;
        public int PassedNoJoinPoint;
        public List<int> Location;
        public int Language;
        public int BuildVersion;
        public string Search;
    }
}