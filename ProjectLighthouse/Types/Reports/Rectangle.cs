using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Reports;

public class Rectangle
{
    [XmlAttribute("t")]
    public int Top { get; set; }

    [XmlAttribute("l")]
    public int Left { get; set; }

    [XmlAttribute("b")]
    public int Bottom { get; set; }

    [XmlAttribute("r")]
    public int Right { get; set; }

}