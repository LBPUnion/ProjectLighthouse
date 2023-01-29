using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Moderation.Reports;

[XmlRoot("marqee")]
public class Marqee
{
    [XmlElement("rect")]
    public Rectangle Rect { get; set; }
}