using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
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
    public List<GameEvent> Events { get; set; }

    public static GameStreamGroup CreateFromGrouping(IGrouping<ActivityGroup, ActivityEntity> group)
    {
        ActivityGroupType type = group.Key.GroupType;
        GameStreamGroup gameGroup = type switch
        {
            ActivityGroupType.Level => new GameSlotStreamGroup
            {
                Slot = new ReviewSlot
                {
                    SlotId = group.Key.TargetId,
                },
            },
            ActivityGroupType.User => new GameUserStreamGroup
            {
                UserId = group.Key.TargetId,
            },
            _ => new GameStreamGroup(),
        };
        gameGroup.Timestamp = new DateTimeOffset(group.Select(a => a.Timestamp).MaxBy(a => a)).ToUnixTimeMilliseconds();
        gameGroup.Type = type;

        List<IGrouping<EventType, ActivityEntity>> eventGroups = group.OrderByDescending(a => a.Timestamp).GroupBy(g => g.Type).ToList();
        //TODO removeme debug
        foreach (IGrouping<EventType, ActivityEntity> bruh in eventGroups)
        {
            Console.WriteLine($@"group key: {bruh.Key}, count={bruh.Count()}");
        }
        gameGroup.Groups = new List<GameStreamGroup>
        {
            new GameUserStreamGroup
            {
                UserId = group.Key.UserId,
                Type = ActivityGroupType.User,
                Timestamp = gameGroup.Timestamp,
                Events = eventGroups.SelectMany(GameEvent.CreateFromActivityGroups).ToList(),
            },
        };

        return gameGroup;
    }
}