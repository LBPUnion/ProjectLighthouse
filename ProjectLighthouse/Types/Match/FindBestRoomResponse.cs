using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Match;

public class FindBestRoomResponse
{
    public int RoomId;

    public List<Player> Players { get; set; }

    public List<List<int>> Slots { get; set; }

    [JsonIgnore]
    public IEnumerable<int> FirstSlot => this.Slots[0];

    [JsonPropertyName("Location")]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public List<string> Locations { get; set; }
}