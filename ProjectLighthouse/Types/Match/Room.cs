using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    public class Room
    {
        public int RoomId;

        public List<User> Players;
        public RoomState State;
        public RoomSlot Slot;

        public bool IsInPod => Slot.SlotType == SlotType.Pod;
        public bool IsLookingForPlayers => this.State == RoomState.DivingIntoLevel || this.State == RoomState.DivingInWaiting;

        public User Host => this.Players[0];

        #nullable enable
        public static bool operator ==(Room? room1, Room? room2)
        {
            if (ReferenceEquals(room1, room2)) return true;
            if ((object?)room1 == null || (object?)room2 == null) return false;

            return room1.RoomId == room2.RoomId;
        }
        public static bool operator !=(Room? room1, Room? room2) => !(room1 == room2);

        public override int GetHashCode() => this.RoomId;
        #nullable disable
    }
}