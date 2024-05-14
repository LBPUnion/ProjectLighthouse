#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Activity;

public class ActivityEntityEventHandler : IEntityEventHandler
{
    public void OnEntityInserted<T>(DatabaseContext database, T entity) where T : class
    {
        ActivityEntity? activity = entity switch
        {
            SlotEntity slot => slot.Type switch
            {
                SlotType.User => new LevelActivityEntity
                {
                    Type = EventType.PublishLevel,
                    SlotId = slot.SlotId,
                    UserId = slot.CreatorId,
                },
                _ => null,
            },
            CommentEntity comment => comment.Type switch
            {
                CommentType.Level => database.Slots.Where(s => s.SlotId == comment.TargetSlotId)
                        .Select(s => s.Type)
                        .FirstOrDefault() switch
                {
                    SlotType.User => new LevelCommentActivityEntity
                    {
                        Type = EventType.CommentOnLevel,
                        CommentId = comment.CommentId,
                        UserId = comment.PosterUserId,
                        SlotId = comment.TargetSlotId ?? throw new NullReferenceException("SlotId in Level comment is null, this shouldn't happen."),
                    },
                    _ => null,
                },
                CommentType.Profile => new UserCommentActivityEntity
                {
                    Type = EventType.CommentOnUser,
                    CommentId = comment.CommentId,
                    UserId = comment.PosterUserId,
                    TargetUserId = comment.TargetUserId ?? throw new NullReferenceException("TargetUserId in User comment is null, this shouldn't happen."),
                }, 
                _ => null,
            },
            PhotoEntity photo => database.Slots.Where(s => s.SlotId == photo.SlotId)
                    .Select(s => s.Type)
                    .FirstOrDefault() switch
            {
                    SlotType.User => new LevelPhotoActivity
                    {
                        Type = EventType.UploadPhoto,
                        PhotoId = photo.PhotoId,
                        UserId = photo.CreatorId,
                        SlotId = photo.SlotId ?? throw new NullReferenceException("SlotId in Photo is null"),
                    },
                    // All other photos (story, moon, pod, etc.)
                    _ => null,
            },
            ScoreEntity score => database.Slots.Where(s => s.SlotId == score.SlotId)
                    .Select(s => s.Type)
                    .FirstOrDefault() switch
            {
                // Don't add story scores or versus scores
                SlotType.User when score.Type != 7 => new ScoreActivityEntity
                {
                    Type = EventType.Score,
                    ScoreId = score.ScoreId,
                    UserId = score.UserId,
                    SlotId = score.SlotId,
                    Points = score.Points,
                },
                _ => null,
            },
            HeartedLevelEntity heartedLevel => database.Slots.Where(s => s.SlotId == heartedLevel.SlotId)
                    .Select(s => s.Type)
                    .FirstOrDefault() switch
            {
                SlotType.User => new LevelActivityEntity
                {
                    Type = EventType.HeartLevel,
                    SlotId = heartedLevel.SlotId,
                    UserId = heartedLevel.UserId,
                },
                _ => null,
            },
            HeartedProfileEntity heartedProfile => new UserActivityEntity
            {
                Type = EventType.HeartUser,
                TargetUserId = heartedProfile.HeartedUserId,
                UserId = heartedProfile.UserId,
            },
            HeartedPlaylistEntity heartedPlaylist => new PlaylistActivityEntity
            {
                Type = EventType.HeartPlaylist,
                PlaylistId = heartedPlaylist.PlaylistId,
                UserId = heartedPlaylist.UserId,
            },
            VisitedLevelEntity visitedLevel => new LevelActivityEntity
            {
                Type = EventType.PlayLevel,
                SlotId = visitedLevel.SlotId,
                UserId = visitedLevel.UserId,
            },
            ReviewEntity review => new ReviewActivityEntity
            {
                Type = EventType.ReviewLevel,
                ReviewId = review.ReviewId,
                UserId = review.ReviewerId,
                SlotId = review.SlotId,
            },
            RatedLevelEntity ratedLevel => new LevelActivityEntity
            {
                Type = ratedLevel.Rating != 0 ? EventType.DpadRateLevel : EventType.RateLevel,
                SlotId = ratedLevel.SlotId,
                UserId = ratedLevel.UserId,
            },
            PlaylistEntity playlist => new PlaylistActivityEntity
            {
                Type = EventType.CreatePlaylist,
                PlaylistId = playlist.PlaylistId,
                UserId = playlist.CreatorId,
            },
            WebsiteAnnouncementEntity announcement => new NewsActivityEntity
            {
                Type = EventType.NewsPost,
                UserId = announcement.PublisherId ?? 0,
                NewsId = announcement.AnnouncementId,
            },
            _ => null,
        };
        InsertActivity(database, activity);
    }

    private static void RemoveDuplicateEvents(DatabaseContext database, ActivityEntity activity)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (activity.Type)
        {
            case EventType.HeartLevel:
            case EventType.UnheartLevel:
            {
                if (activity is not LevelActivityEntity levelActivity) break;

                DeleteActivity(a => a.TargetSlotId == levelActivity.SlotId);
                break;
            }
            case EventType.HeartUser:
            case EventType.UnheartUser:
            {
                if (activity is not UserActivityEntity userActivity) break;

                DeleteActivity(a => a.TargetUserId == userActivity.TargetUserId);
                break;
            }
            case EventType.HeartPlaylist:
            {
                if (activity is not PlaylistActivityEntity playlistActivity) break;

                DeleteActivity(a => a.TargetPlaylistId == playlistActivity.PlaylistId);
                break;
            }
        }

        return;

        void DeleteActivity(Expression<Func<ActivityDto, bool>> predicate)
        {
            database.Activities.ToActivityDto()
                .Where(a => a.Activity.UserId == activity.UserId)
                .Where(a => a.Activity.Type == activity.Type)
                .Where(predicate)
                .Select(a => a.Activity)
                .ExecuteDelete();
        }
    }

    private static void InsertActivity(DatabaseContext database, ActivityEntity? activity)
    {
        if (activity == null) return;

        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return;

        Logger.Debug("Inserting activity: " + activity.GetType().Name, LogArea.Activity);

        RemoveDuplicateEvents(database, activity);

        activity.Timestamp = DateTime.UtcNow;
        database.Activities.Add(activity);
        database.SaveChanges();
    }

    public void OnEntityChanged<T>(DatabaseContext database, T origEntity, T currentEntity) where T : class
    {
        ActivityEntity? activity = null;
        switch (currentEntity)
        {
            case VisitedLevelEntity visitedLevel:
            {
                if (origEntity is not VisitedLevelEntity oldVisitedLevel) break;

                if (Plays(oldVisitedLevel) >= Plays(visitedLevel)) break;

                activity = new LevelActivityEntity
                {
                    Type = EventType.PlayLevel,
                    SlotId = visitedLevel.SlotId,
                    UserId = visitedLevel.UserId,
                };
                break;

                int Plays(VisitedLevelEntity entity) => entity.PlaysLBP1 + entity.PlaysLBP2 + entity.PlaysLBP3;
            }
            case ScoreEntity score:
            {
                if (origEntity is not ScoreEntity oldScore) break;

                // don't track versus levels
                if (oldScore.Type == 7) break;

                SlotType slotType = database.Slots.Where(s => s.SlotId == score.SlotId)
                    .Select(s => s.Type)
                    .FirstOrDefault();

                if (slotType != SlotType.User) break;

                if (oldScore.Points > score.Points) break;

                activity = new ScoreActivityEntity
                {
                    Type = EventType.Score,
                    ScoreId = score.ScoreId,
                    SlotId = score.SlotId,
                    UserId = score.UserId,
                    Points = score.Points,
                };

                break;
            }
            case SlotEntity slotEntity:
            {
                if (origEntity is not SlotEntity oldSlotEntity) break;

                bool oldIsTeamPick = oldSlotEntity.TeamPickTime != 0;
                bool newIsTeamPick = slotEntity.TeamPickTime != 0;

                switch (oldIsTeamPick)
                {
                    // When a level is team picked
                    case false when newIsTeamPick:
                        activity = new LevelActivityEntity
                        {
                            Type = EventType.MMPickLevel,
                            SlotId = slotEntity.SlotId,
                            UserId = slotEntity.CreatorId,
                        };
                        break;
                    // When a level has its team pick removed then remove the corresponding activity
                    case true when !newIsTeamPick:
                        database.Activities.OfType<LevelActivityEntity>()
                            .Where(a => a.Type == EventType.MMPickLevel)
                            .Where(a => a.SlotId == slotEntity.SlotId)
                            .ExecuteDelete();
                        break;
                    default:
                    {
                        if (oldSlotEntity.SlotId == slotEntity.SlotId &&
                            slotEntity.Type == SlotType.User &&
                            oldSlotEntity.LastUpdated != slotEntity.LastUpdated)
                        {
                            activity = new LevelActivityEntity
                            {
                                Type = EventType.PublishLevel,
                                SlotId = slotEntity.SlotId,
                                UserId = slotEntity.CreatorId,
                            };
                        }

                        break;
                    }
                }
                break;
            }
            case CommentEntity comment:
            {
                if (origEntity is not CommentEntity oldComment) break;

                if (comment.TargetSlotId != null)
                {
                    SlotType slotType = database.Slots.Where(s => s.SlotId == comment.TargetSlotId)
                        .Select(s => s.Type)
                        .FirstOrDefault();
                    if (slotType != SlotType.User) break;
                }

                if (oldComment.Deleted || !comment.Deleted) break;

                if (comment.Type != CommentType.Level) break;

                activity = new CommentActivityEntity
                {
                    Type = EventType.DeleteLevelComment,
                    CommentId = comment.CommentId,
                    UserId = comment.PosterUserId,
                };
                break;
            }
            case PlaylistEntity playlist:
            {
                if (origEntity is not PlaylistEntity oldPlaylist) break;

                int[] newSlots = playlist.SlotIds;
                int[] oldSlots = oldPlaylist.SlotIds;
                Logger.Debug($@"Old playlist slots: {string.Join(",", oldSlots)}", LogArea.Activity);
                Logger.Debug($@"New playlist slots: {string.Join(",", newSlots)}", LogArea.Activity);

                int[] addedSlots = newSlots.Except(oldSlots).ToArray();

                Logger.Debug($@"Added playlist slots: {string.Join(",", addedSlots)}", LogArea.Activity);

                // If no new level have been added
                if (addedSlots.Length == 0) break;

                // Normally events only need 1 resulting ActivityEntity but here
                // we need multiple, so we have to do the inserting ourselves.
                foreach (int slotId in addedSlots)
                {
                    ActivityEntity entity = new PlaylistWithSlotActivityEntity
                    {
                        Type = EventType.AddLevelToPlaylist,
                        PlaylistId = playlist.PlaylistId,
                        SlotId = slotId,
                        UserId = playlist.CreatorId,
                    };
                    InsertActivity(database, entity);
                }

                break;
            }
        }

        InsertActivity(database, activity);
    }

    public void OnEntityDeleted<T>(DatabaseContext database, T entity) where T : class
    {
        ActivityEntity? activity = entity switch
        {
            HeartedLevelEntity heartedLevel => database.Slots.Where(s => s.SlotId == heartedLevel.SlotId)
                    .Select(s => s.Type)
                    .FirstOrDefault() switch
            {
                SlotType.User => new LevelActivityEntity
                {
                    Type = EventType.UnheartLevel,
                    SlotId = heartedLevel.SlotId,
                    UserId = heartedLevel.UserId,
                },
                _ => null,
            },
            HeartedProfileEntity heartedProfile => new UserActivityEntity
            {
                Type = EventType.UnheartUser,
                TargetUserId = heartedProfile.HeartedUserId,
                UserId = heartedProfile.UserId,
            },
            _ => null,
        };
        InsertActivity(database, activity);
    }
}