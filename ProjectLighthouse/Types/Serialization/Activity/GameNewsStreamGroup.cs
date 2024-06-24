using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

public class GameNewsStreamGroup : GameStreamGroup
{
    [XmlElement("news_id")]
    public int NewsId { get; set; }
}