using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Moderation.Reports;

[XmlRoot("player")]
public class ReportPlayer
{
    [XmlElement("id")]
    public string Name { get; set; }

    [XmlElement("rect")]
    public Rectangle Location { get; set; }

    [XmlAttribute("reporter")]
    public bool Reporter { get; set; }

    [XmlAttribute("ingamenow")]
    public bool InGame { get; set; }

    [XmlAttribute("playerNumber")]
    public int PlayerNum { get; set; }

}