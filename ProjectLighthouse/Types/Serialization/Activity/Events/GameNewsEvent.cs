using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameNewsEvent : GameEvent
{
    [XmlElement("news_id")]
    public int NewsId { get; set; }
}