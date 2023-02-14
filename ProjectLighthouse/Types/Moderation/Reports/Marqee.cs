using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Moderation.Reports;

[XmlRoot("marqee")]
public class Marqee
{
    [XmlElement("rect")]
    public Rectangle Rect { get; set; }
}