using System.Collections.Generic;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("slot")]
public class SlotResourceResponse : ILbpSerializable
{

    public SlotResourceResponse(List<string> resources)
    {
        this.Resources = resources;
    }

    [XmlAttribute("type")]
    public string Type { get; set; } = "user";

    [XmlArray("resources")]
    [XmlArrayItem("resource")]
    public List<string> Resources { get; set; }

}