using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Match;

public class Room
{
    [JsonIgnore]
    public List<User> Players { get; set; }

    public int RoomId { get; set; }

    [JsonIgnore]
    public GameVersion RoomVersion { get; set; }

    [JsonIgnore]
    public Platform RoomPlatform { get; set; }

    public RoomSlot Slot { get; set; }
    public RoomState State { get; set; }

    [JsonIgnore]
    public bool IsInPod => this.Slot.SlotType == SlotType.Pod;

    [JsonIgnore]
    public bool IsLookingForPlayers => this.State == RoomState.PlayingLevel || this.State == RoomState.DivingInWaiting;

    [JsonIgnore]
    public User Host => this.Players[0];

    public int PlayerCount => this.Players.Count;

    #nullable enable
    public override bool Equals(object? obj)
    {
        if (obj is Room room) return room.RoomId == this.RoomId;

        return false;
    }

    public static bool operator ==(Room? room1, Room? room2)
    {
        if (ReferenceEquals(room1, room2)) return true;
        if ((object?)room1 == null || (object?)room2 == null) return false;

        return room1.RoomId == room2.RoomId;
    }
    public static bool operator !=(Room? room1, Room? room2) => !(room1 == room2);

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() => this.RoomId;
    #nullable disable
}