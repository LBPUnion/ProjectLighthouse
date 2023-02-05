using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Redis.OM.Modeling;

namespace LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

[Document(StorageType = StorageType.Json, Prefixes = new[]{"Room",})]
public class Room
{
    private int roomId;

    [Indexed]
    public int RoomId {
        get => this.roomId;
        set {
            this.RedisId = value.ToString();
            this.roomId = value;
        }
    }

    [RedisIdField] 
    public string RedisId { get; set; }

    [Indexed]
    public List<int> PlayerIds { get; set; }

    [Indexed]
    public GameVersion RoomVersion { get; set; }

    [Indexed]
    public Platform RoomPlatform { get; set; }

    [Indexed]
    public RoomSlot Slot { get; set; }

    [Indexed]
    public RoomState State { get; set; }

    [JsonIgnore]
    [Indexed]
    public bool IsLookingForPlayers => this.State == RoomState.PlayingLevel || this.State == RoomState.DivingInWaiting;

    [JsonIgnore]
    public int HostId
    {
        get
        {
            if (this.PlayerIds.Count > 0)
                return this.PlayerIds[0];
            
            return -1;
        }
    }

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