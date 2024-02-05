using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;
using LBPUnion.ProjectLighthouse.Types.Notifications;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("notification")]
public class GameNotification : ILbpSerializable
{
    [XmlAttribute("type")]
    public NotificationType Type { get; set; }

    [XmlElement("text")]
    public string Text { get; set; } = "";

    public static GameNotification CreateFromEntity(NotificationEntity notification) => new()
    {
        Type = notification.Type,
        Text = notification.Text,
    };
}