using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Matchmaking.Rooms;

public class RoomSlot
{
    public int SlotId { get; set; }
    public SlotType SlotType { get; set; }

    public static readonly RoomSlot PodSlot = new()
    {
        SlotType = SlotType.Pod,
        SlotId = 0,
    };
}