using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Notifications;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("notification")]
public class GameNotification : ILbpSerializable
{
    [XmlIgnore]
    public int UserId { get; set; }

    [XmlAttribute("type")]
    public NotificationType Type { get; set; }

    [XmlElement("text")]
    public string Text { get; set; } = null!;
}