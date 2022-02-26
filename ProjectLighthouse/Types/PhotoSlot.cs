using System;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types;

[XmlRoot("slot")]
public class PhotoSlot
{
    [XmlAttribute("type")]
    public string SlotType { get; set; }

    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlElement("rootLevel")]
    public string RootLevel { get; set; }

    [XmlElement("name")]
    public string LevelName { get; set; }
}