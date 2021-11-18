using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    // Schema is the EXACT SAME as CreateRoom (but cant be a subclass here), so see comments there for details
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    public class FindBestRoom : IMatchData
    {
        public List<string> Players { get; set; }

        public List<string> Reservations { get; set; }
        public List<List<int>> Slots { get; set; }

        [JsonIgnore]
        public IEnumerable<int> FirstSlot => this.Slots[0];

        public List<int> NAT;
        public RoomState RoomState;
        public int HostMood;
        public int PassedNoJoinPoint;
        public List<int> Location;
        public int Language;
        public int BuildVersion;
        public string Search;
    }
}