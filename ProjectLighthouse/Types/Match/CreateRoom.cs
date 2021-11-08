using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    public class CreateRoom : IMatchData
    {
        public List<string> Players { get; set; }
        public List<string> Reservations { get; set; }

        //           v slot type, 1 = 2974
        // "Slots":[[5,0]]
        //             ^ slot id
        // no idea why this is an array, but we'll work with it i suppose
        public List<List<int>> Slots { get; set; }

        [JsonIgnore]
        public List<int> FirstSlot => this.Slots[0];
    }
}