using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    public class Room
    {
        public List<User> Players;
        public RoomState State;
        public RoomSlot Slot;

        public bool IsInPod => Slot.SlotType == SlotType.Pod;
        public bool IsLookingForPlayers => this.State == RoomState.DivingIntoLevel || this.State == RoomState.DivingInWaiting;
    }
}