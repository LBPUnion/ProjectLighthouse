using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    public class CreateRoom : IMatchData
    {
        //[CreateRoom,["Players":["LumaLivy"],"Reservations":["0"],"NAT":[2],"Slots":[[1,3]],"RoomState":0,"HostMood":1,"PassedNoJoinPoint":0,"Location":[0x7f000001],"Language":1,"BuildVersion":289,"Search":""]]
        public List<string> Players;
        public List<string> Reservations;
        public List<int> NAT;
        public List<List<int>> Slots;
        public int RoomState;
        public int HostMood;
        public int PassedNoJoinPoint;
        public List<int> Location;
        public int Language;
        public int BuildVersion;
        public string Search;
    }
}