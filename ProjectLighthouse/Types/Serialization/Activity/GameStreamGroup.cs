using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

/// <summary>
/// Top level groups generally contain all events for a given level or user
/// <para>
/// The sub-groups are always <see cref="GameUserStreamGroup"/> and contain all activities from a single user
/// for the top level group entity
/// </para> 
/// </summary>
[XmlInclude(typeof(GameUserStreamGroup))]
[XmlInclude(typeof(GameSlotStreamGroup))]
[XmlInclude(typeof(GamePlaylistStreamGroup))]
[XmlInclude(typeof(GameNewsStreamGroup))]
public class GameStreamGroup : ILbpSerializable
{
    [XmlAttribute("type")]
    public ActivityGroupType Type { get; set; }

    [XmlElement("timestamp")]
    public long Timestamp { get; set; }

    [XmlArray("subgroups")]
    [XmlArrayItem("group")]
    [DefaultValue(null)]
    public List<GameStreamGroup> Groups { get; set; }

    [XmlArray("events")]
    [XmlArrayItem("event")]
    [DefaultValue(null)]
    // ReSharper disable once MemberCanBePrivate.Global
    // (the serializer can't see this if it's private)
    public List<GameEvent> Events { get; set; }

    public static GameStreamGroup CreateFromGroup(OuterActivityGroup group)
    {
        GameStreamGroup gameGroup = CreateGroup(group.Key.GroupType,
            group.Key.TargetId,
            streamGroup =>
            {
                streamGroup.Timestamp = group.Groups
                    .Max(g => g.MaxBy(a => a.Activity.Timestamp)?.Activity.Timestamp ?? group.Key.Timestamp)
                    .ToUnixTimeMilliseconds();
            });

        gameGroup.Groups = new List<GameStreamGroup>(group.Groups.Select(g => CreateGroup(g.Key.Type,
                g.Key.TargetId,
                streamGroup =>
                {
                    streamGroup.Timestamp =
                        g.MaxBy(a => a.Activity.Timestamp).Activity.Timestamp.ToUnixTimeMilliseconds();
                    streamGroup.Events = GameEvent.CreateFromActivities(g).ToList();
                }))
            .ToList());

        return gameGroup;
    }

    private static GameStreamGroup CreateGroup
        (ActivityGroupType type, int targetId, Action<GameStreamGroup> groupAction)
    {
        GameStreamGroup gameGroup = type switch
        {
            ActivityGroupType.Level or ActivityGroupType.TeamPick => new GameSlotStreamGroup
            {
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            ActivityGroupType.User => new GameUserStreamGroup
            {
                UserId = targetId,
            },
            ActivityGroupType.Playlist => new GamePlaylistStreamGroup
            {
                PlaylistId = targetId,
            },
            ActivityGroupType.News => new GameNewsStreamGroup
            {
                NewsId = targetId,
            },
            _ => new GameStreamGroup(),
        };
        gameGroup.Type = type;
        groupAction(gameGroup);
        return gameGroup;
    }
}