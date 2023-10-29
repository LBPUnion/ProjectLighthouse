using System;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Notifications;

public enum NotificationType
{
    [Obsolete("This notification type is ignored by the game and does nothing.")]
    [XmlEnum("mmPick")]
    MMPick,

    [Obsolete("This notification type is ignored by the game and does nothing.")]
    [XmlEnum("playsOnSlot")]
    PlaysOnSlot,

    [Obsolete("This notification type is ignored by the game and does nothing.")]
    [XmlEnum("top100Hottest")]
    Top100Hottest,

    /// <summary>
    ///     Displays a moderation notification upon login and in LBP Messages.
    /// </summary>
    [XmlEnum("moderationNotification")]
    ModerationNotification,

    [Obsolete("This notification type is ignored by the game and does nothing.")]
    [XmlEnum("commentOnSlot")]
    CommentOnSlot,

    [Obsolete("This notification type is ignored by the game and does nothing.")]
    [XmlEnum("heartsOnSlot")]
    HeartsOnSlot,

    [Obsolete("This notification type is ignored by the game and does nothing.")]
    [XmlEnum("heartedAsAuthor")]
    HeartedAsAuthor,
}