using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Review;

[XmlRoot("slot")]
public class ReviewSlot : ILbpSerializable
{
    [XmlAttribute("type")]
    public SlotType SlotType { get; set; }

    [XmlText]
    public int SlotId { get; set; }

    public static ReviewSlot CreateFromEntity(SlotEntity slot) =>
        new()
        {
            SlotType = slot.Type,
            SlotId = slot.Type == SlotType.User ? slot.SlotId : slot.InternalSlotId,
        };
}