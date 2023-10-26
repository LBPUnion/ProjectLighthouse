using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;

namespace LBPUnion.ProjectLighthouse.Types.Notifications;

public enum NotificationType
{
    [XmlEnum("mmPick")]
    MMPick,

    [XmlEnum("playsOnSlot")]
    PlaysOnSlot,

    [XmlEnum("top100Hottest")]
    Top100Hottest,

    [XmlEnum("moderationNotification")]
    ModerationNotification,

    [XmlEnum("commentOnSlot")]
    CommentOnSlot,

    [XmlEnum("heartsOnSlot")]
    HeartsOnSlot,

    [XmlEnum("heartedAsAuthor")]
    HeartedAsAuthor,
}