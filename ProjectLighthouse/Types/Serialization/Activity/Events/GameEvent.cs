using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

[XmlInclude(typeof(GameCommentEvent))]
[XmlInclude(typeof(GamePhotoUploadEvent))]
[XmlInclude(typeof(GamePlayLevelEvent))]
[XmlInclude(typeof(GameReviewEvent))]
[XmlInclude(typeof(GameScoreEvent))]
[XmlInclude(typeof(GameHeartLevelEvent))]
[XmlInclude(typeof(GameHeartUserEvent))]
public class GameEvent : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    private int UserId { get; set; }

    [XmlAttribute("type")]
    public EventType Type { get; set; }

    [XmlElement("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("actor")]
    public string Username { get; set; }

    protected async Task PrepareSerialization(DatabaseContext database)
    {
        Console.WriteLine($@"SERIALIZATION!! {this.UserId} - {this.GetHashCode()}");
        UserEntity user = await database.Users.FindAsync(this.UserId);
        if (user == null) return;
        this.Username = user.Username;
    }

    public static IEnumerable<GameEvent> CreateFromActivityGroups(IGrouping<EventType, ActivityEntity> group)
    {
        List<GameEvent> events = new();

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // Events with Count need special treatment
        switch (group.Key)
        {
            case EventType.PlayLevel:
            {
                if (group.First() is not LevelActivityEntity levelActivity) break;

                events.Add(new GamePlayLevelEvent
                {
                    Slot = new ReviewSlot
                    {
                        SlotId = levelActivity.SlotId,
                    },
                    Count = group.Count(),
                    UserId = levelActivity.UserId,
                    Timestamp = levelActivity.Timestamp.ToUnixTimeMilliseconds(),
                    Type = levelActivity.Type,
                });
                break;
            }
            case EventType.PublishLevel:
            {
                if (group.First() is not LevelActivityEntity levelActivity) break;

                events.Add(new GamePublishLevelEvent
                {
                    Slot = new ReviewSlot
                    {
                        SlotId = levelActivity.SlotId,
                    },
                    Count = group.Count(),
                    UserId = levelActivity.UserId,
                    Timestamp = levelActivity.Timestamp.ToUnixTimeMilliseconds(),
                    Type = levelActivity.Type,
                });
                break;
            }
            // Everything else can be handled as normal
            default: events.AddRange(group.Select(CreateFromActivity));
                break;
        }
        return events.AsEnumerable();
    }

    private static GameEvent CreateFromActivity(ActivityEntity activity)
    {
        GameEvent gameEvent = activity.Type switch
        {
            EventType.PlayLevel => new GamePlayLevelEvent
            {
                Slot = new ReviewSlot
                {
                    SlotId = ((LevelActivityEntity)activity).SlotId,
                },
            },
            EventType.CommentOnLevel => new GameSlotCommentEvent
            {
                CommentId = ((CommentActivityEntity)activity).CommentId,
            },
            EventType.CommentOnUser => new GameUserCommentEvent
            {
                CommentId = ((CommentActivityEntity)activity).CommentId,
            },
            EventType.HeartUser or EventType.UnheartUser => new GameHeartUserEvent
            {
                TargetUserId = ((UserActivityEntity)activity).TargetUserId,
            },
            EventType.HeartLevel or EventType.UnheartLevel => new GameHeartLevelEvent
            {
                TargetSlot = new ReviewSlot
                {
                    SlotId = ((LevelActivityEntity)activity).SlotId,
                },
            },
            _ => new GameEvent(),
        };
        gameEvent.UserId = activity.UserId;
        gameEvent.Type = activity.Type;
        gameEvent.Timestamp = activity.Timestamp.ToUnixTimeMilliseconds();
        return gameEvent;
    }
}