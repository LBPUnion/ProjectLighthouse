using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Slot;

[XmlRoot("slot")]
public struct SlotResourceResponse : ILbpSerializable
{
    public SlotResourceResponse(List<string> resources)
    {
        this.Resources = resources;
    }

    [XmlAttribute("type")]
    public string Type { get; set; } = "user";

    [XmlElement("resource")]
    public List<string> Resources { get; set; }

}