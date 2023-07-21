using System;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Activity;

public class ActivityGroup
{
    public DateTime Timestamp { get; set; }
    public int UserId { get; set; }
    public int? TargetSlotId { get; set; }
    public int? TargetUserId { get; set; }
    public int? TargetPlaylistId { get; set; }

    public int TargetId =>
        this.GroupType switch
        {
            ActivityGroupType.User => this.TargetUserId ?? 0,
            ActivityGroupType.Level => this.TargetSlotId ?? 0,
            ActivityGroupType.Playlist => this.TargetPlaylistId ?? 0,
            _ => this.UserId,
        };

    public ActivityGroupType GroupType =>
        this.TargetSlotId != 0
            ? ActivityGroupType.Level
            : this.TargetUserId != 0
                ? ActivityGroupType.User
                : ActivityGroupType.Playlist;
}

public enum ActivityGroupType
{
    [XmlEnum("user")]
    User,

    [XmlEnum("slot")]
    Level,

    [XmlEnum("playlist")]
    Playlist,
}