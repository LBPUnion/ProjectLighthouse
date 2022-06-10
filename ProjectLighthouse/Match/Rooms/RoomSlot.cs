using LBPUnion.ProjectLighthouse.Levels;

namespace LBPUnion.ProjectLighthouse.Match.Rooms;

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