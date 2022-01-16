using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    public class Room
    {
        public List<User> Players;
        public int RoomId;
        public RoomSlot Slot;
        public RoomState State;

        public bool IsInPod => this.Slot.SlotType == SlotType.Pod;
        public bool IsLookingForPlayers => this.State == RoomState.PlayingLevel || this.State == RoomState.DivingInWaiting;

        public User Host => this.Players[0];

        public GameVersion RoomVersion;

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
}