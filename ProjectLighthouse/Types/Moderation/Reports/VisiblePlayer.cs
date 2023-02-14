using System;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Moderation.Reports;

[XmlRoot("visibleBadge")]
[Serializable]
public class VisiblePlayer
{
    [XmlElement("id")]
    public string Name { get; set; }

    [XmlElement("hash")]
    public string Hash { get; set; }

    [XmlElement("rect")]
    public Rectangle Bounds { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

}