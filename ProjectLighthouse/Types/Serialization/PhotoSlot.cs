#nullable enable
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("slot")]
public class PhotoSlot : ILbpSerializable
{
    [XmlAttribute("type")]
    public SlotType SlotType { get; set; }

    [XmlElement("id")]
    public int SlotId { get; set; }

    [DefaultValue("")]
    [XmlElement("rootLevel")]
    public string? RootLevel { get; set; }

    [DefaultValue("")]
    [XmlElement("name")]
    public string? LevelName { get; set; }
}