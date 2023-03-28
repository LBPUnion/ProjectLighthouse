using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("text_search")]
public class TextSearch
{
    [XmlElement("url")]
    public string Url { get; set; } = "/slots/searchLBP3";
}