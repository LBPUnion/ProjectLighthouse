#nullable enable
using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;
#if DEBUG
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
#endif

namespace LBPUnion.ProjectLighthouse.Types.Activity;

//TODO implement missing event triggers
public class ActivityEntityEventHandler : IEntityEventHandler
{
    public void OnEntityInserted<T>(DatabaseContext database, T entity) where T : class
    {
        Console.WriteLine($@"OnEntityInserted: {entity.GetType().Name}");
        ActivityEntity? activity = entity switch
        {
            SlotEntity slot => new LevelActivityEntity
            {
                Type = EventType.PublishLevel,
                SlotId = slot.SlotId,
                UserId = slot.CreatorId,
            },
            CommentEntity comment => new CommentActivityEntity
            {
                Type = comment.Type == CommentType.Level ? EventType.CommentOnLevel : EventType.CommentOnUser,
                CommentId = comment.CommentId,
                UserId = comment.PosterUserId,
            },
            PhotoEntity photo => new PhotoActivityEntity
            {
                Type = EventType.UploadPhoto,
                PhotoId = photo.PhotoId,
                UserId = photo.CreatorId,
            },
            ScoreEntity score => new ScoreActivityEntity
            {
                Type = EventType.Score,
                ScoreId = score.ScoreId,
                UserId = score.UserId,
            },
            HeartedLevelEntity heartedLevel => new LevelActivityEntity
            {
                Type = EventType.HeartLevel,
                SlotId = heartedLevel.SlotId,
                UserId = heartedLevel.UserId,
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

    private static void InsertActivity(DatabaseContext database, ActivityEntity? activity)
    {
        if (activity == null) return;

        Console.WriteLine("Inserting activity: " + activity.GetType().Name);

        activity.Timestamp = DateTime.UtcNow;
        database.Activities.Add(activity);
        database.SaveChanges();
    }

    public void OnEntityChanged<T>(DatabaseContext database, T origEntity, T currentEntity) where T : class
    {
        #if DEBUG
        foreach (PropertyInfo propInfo in currentEntity.GetType().GetProperties())
        {
            if (!propInfo.CanRead || !propInfo.CanWrite) continue;

            if (propInfo.CustomAttributes.Any(c => c.AttributeType == typeof(NotMappedAttribute))) continue;

            object? origVal = propInfo.GetValue(origEntity);
            object? newVal = propInfo.GetValue(currentEntity);
            if ((origVal == null && newVal == null) || (origVal != null && newVal != null && origVal.Equals(newVal)))
                continue;

            Console.WriteLine($@"Value for {propInfo.Name} changed");
            Console.WriteLine($@"Orig val: {origVal?.ToString() ?? "null"}");
            Console.WriteLine($@"New val: {newVal?.ToString() ?? "null"}");
        }
        Console.WriteLine($@"OnEntityChanged: {currentEntity.GetType().Name}");
        #endif

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
            case SlotEntity slotEntity:
            {
                if (origEntity is not SlotEntity oldSlotEntity) break;

                switch (oldSlotEntity.TeamPick)
                {
                    // When a level is team picked
                    case false when slotEntity.TeamPick:
                        activity = new LevelActivityEntity
                        {
                            Type = EventType.MMPickLevel,
                            SlotId = slotEntity.SlotId,
                            UserId = slotEntity.CreatorId,
                        };
                        break;
                    // When a level has its team pick removed then remove the corresponding activity
                    case true when !slotEntity.TeamPick:
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
                Console.WriteLine($@"Old playlist slots: {string.Join(",", oldSlots)}");
                Console.WriteLine($@"New playlist slots: {string.Join(",", newSlots)}");

                int[] addedSlots = newSlots.Except(oldSlots).ToArray();

                Console.WriteLine($@"Added playlist slots: {string.Join(",", addedSlots)}");

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
        Console.WriteLine($@"OnEntityDeleted: {entity.GetType().Name}");
        ActivityEntity? activity = entity switch
        {
            HeartedLevelEntity heartedLevel => new LevelActivityEntity
            {
                Type = EventType.UnheartLevel,
                SlotId = heartedLevel.SlotId,
                UserId = heartedLevel.UserId,
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