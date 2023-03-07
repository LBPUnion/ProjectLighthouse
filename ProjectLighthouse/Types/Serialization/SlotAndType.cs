using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("slot_id")]
public class SlotAndType
{

    public SlotAndType(int slotId, string slotType)
    {
        this.SlotId = slotId;
        this.Type = slotType;
    }

    [XmlText]
    public int SlotId { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

}