using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Slot;

[XmlRoot("slot")]
public class MinimalSlot : ILbpSerializable
{
    [XmlElement("type")]
    public SlotType Type { get; set; }

    [XmlElement("id")]
    public int SlotId { get; set; }

    public MinimalSlot CreateFromEntity(SlotEntity slot) =>
        new()
        {
            Type = slot.Type,
            SlotId = slot.SlotId,
        };
}