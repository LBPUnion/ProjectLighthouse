using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

[XmlInclude(typeof(GameCommentEvent))]
[XmlInclude(typeof(GamePhotoUploadEvent))]
[XmlInclude(typeof(GamePlayLevelEvent))]
[XmlInclude(typeof(GameReviewEvent))]
[XmlInclude(typeof(GameScoreEvent))]
[XmlInclude(typeof(GameHeartLevelEvent))]
[XmlInclude(typeof(GameHeartUserEvent))]
[XmlInclude(typeof(GameHeartPlaylistEvent))]
[XmlInclude(typeof(GameReviewEvent))]
[XmlInclude(typeof(GamePublishLevelEvent))]
[XmlInclude(typeof(GameRateLevelEvent))]
[XmlInclude(typeof(GameDpadRateLevelEvent))]
[XmlInclude(typeof(GameTeamPickLevelEvent))]
[XmlInclude(typeof(GameNewsEvent))]
[XmlInclude(typeof(GameCreatePlaylistEvent))]
[XmlInclude(typeof(GameAddLevelToPlaylistEvent))]
public class GameEvent : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    protected int UserId { get; set; }

    [XmlAttribute("type")]
    public EventType Type { get; set; }

    [XmlElement("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("actor")]
    [DefaultValue(null)]
    public string Username { get; set; }

    protected async Task PrepareSerialization(DatabaseContext database)
    {
        #if DEBUG
        Logger.Debug($@"EVENT SERIALIZATION!! userId: {this.UserId} - hashCode: {this.GetHashCode()}", LogArea.Activity);
        #endif
        UserEntity user = await database.Users.FindAsync(this.UserId);
        if (user == null) return;
        this.Username = user.Username;
    }

    public static IEnumerable<GameEvent> CreateFromActivities(IEnumerable<ActivityDto> activities)
    {
        List<GameEvent> events = new();
        List<IGrouping<EventType, ActivityDto>> typeGroups = activities.GroupBy(g => g.Activity.Type).ToList();
        foreach (IGrouping<EventType, ActivityDto> typeGroup in typeGroups)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // Events with Count need special treatment
            switch (typeGroup.Key)
            {
                case EventType.PlayLevel:
                {
                    if (typeGroup.First().Activity is not LevelActivityEntity levelActivity) break;

                    events.Add(new GamePlayLevelEvent
                    {
                        Slot = new ReviewSlot
                        {
                            SlotId = levelActivity.SlotId,
                        },
                        Count = typeGroup.Count(),
                        UserId = levelActivity.UserId,
                        Timestamp = levelActivity.Timestamp.ToUnixTimeMilliseconds(),
                        Type = levelActivity.Type,
                    });
                    break;
                }
                // Everything else can be handled as normal
                default:
                    events.AddRange(typeGroup.Select(CreateFromActivity).Where(a => a != null));
                    break;
            }
        }

        return events.AsEnumerable();
    }

    private static bool IsValidActivity(ActivityEntity activity)
    {
        return activity switch
        {
            CommentActivityEntity => activity.Type is EventType.CommentOnLevel or EventType.CommentOnUser
                or EventType.DeleteLevelComment,
            LevelActivityEntity => activity.Type is EventType.PlayLevel or EventType.HeartLevel
                or EventType.UnheartLevel or EventType.DpadRateLevel or EventType.RateLevel or EventType.MMPickLevel
                or EventType.PublishLevel or EventType.TagLevel,
            NewsActivityEntity => activity.Type is EventType.NewsPost,
            PhotoActivityEntity => activity.Type is EventType.UploadPhoto,
            PlaylistActivityEntity => activity.Type is EventType.CreatePlaylist or EventType.HeartPlaylist,
            PlaylistWithSlotActivityEntity => activity.Type is EventType.AddLevelToPlaylist,
            ReviewActivityEntity => activity.Type is EventType.ReviewLevel,
            ScoreActivityEntity => activity.Type is EventType.Score,
            UserActivityEntity => activity.Type is EventType.HeartUser or EventType.UnheartUser
                or EventType.CommentOnUser,
            _ => false,
        };
    }

    private static GameEvent CreateFromActivity(ActivityDto activity)
    {
        if (!IsValidActivity(activity.Activity))
        {
            Logger.Error(@"Invalid Activity: " + activity.Activity.ActivityId, LogArea.Activity);
            return null;
        }

        int targetId = activity.TargetId;

        GameEvent gameEvent = activity.Activity.Type switch
        {
            EventType.PlayLevel => new GamePlayLevelEvent
            {
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.HeartLevel or EventType.UnheartLevel => new GameHeartLevelEvent
            {
                TargetSlot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.DpadRateLevel => new GameDpadRateLevelEvent
            {
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.Score => new GameScoreEvent
            {
                ScoreId = ((ScoreActivityEntity)activity.Activity).ScoreId,
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.RateLevel => new GameRateLevelEvent
            {
                Slot = new ReviewSlot
                {
                    SlotId = targetId
                },
            },
            EventType.CommentOnLevel => new GameSlotCommentEvent
            {
                CommentId = ((CommentActivityEntity)activity.Activity).CommentId,
            },
            EventType.CommentOnUser => new GameUserCommentEvent
            {
                CommentId = ((CommentActivityEntity)activity.Activity).CommentId,
            },
            EventType.HeartUser or EventType.UnheartUser => new GameHeartUserEvent
            {
                TargetUserId = targetId,
            },
            EventType.ReviewLevel => new GameReviewEvent
            {
                ReviewId = ((ReviewActivityEntity)activity.Activity).ReviewId,
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.UploadPhoto => new GamePhotoUploadEvent
            {
                PhotoId = ((PhotoActivityEntity)activity.Activity).PhotoId,
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.MMPickLevel => new GameTeamPickLevelEvent
            {
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
            },
            EventType.PublishLevel => new GamePublishLevelEvent
            {
                Slot = new ReviewSlot
                {
                    SlotId = targetId,
                },
                Count = 1,
            },
            EventType.NewsPost => new GameNewsEvent
            {
                NewsId = targetId,
            },
            EventType.CreatePlaylist => new GameCreatePlaylistEvent
            {
                TargetPlaylistId = targetId,
            },
            EventType.HeartPlaylist => new GameHeartPlaylistEvent
            {
                TargetPlaylistId = targetId,
            },
            EventType.AddLevelToPlaylist => new GameAddLevelToPlaylistEvent
            {
                TargetPlaylistId = targetId,
                Slot = new ReviewSlot
                {
                    SlotId = ((PlaylistWithSlotActivityEntity)activity.Activity).SlotId,
                },
            },
            _ => new GameEvent(),
        };
        gameEvent.UserId = activity.Activity.UserId;
        gameEvent.Type = activity.Activity.Type;
        gameEvent.Timestamp = activity.Activity.Timestamp.ToUnixTimeMilliseconds();
        return gameEvent;
    }
}