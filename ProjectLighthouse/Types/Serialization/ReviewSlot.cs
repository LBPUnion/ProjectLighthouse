using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("slot")]
public class ReviewSlot : ILbpSerializable
{
    [XmlAttribute("type")]
    public SlotType SlotType { get; set; }

    [XmlText]
    public int SlotId { get; set; }
}