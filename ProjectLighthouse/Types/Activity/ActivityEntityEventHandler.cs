#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;

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
                //TODO merge score migration
                // UserId = int.Parse(score.PlayerIds[0]),
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
            VisitedLevelEntity visitedLevel => new LevelActivityEntity
            {
                Type = EventType.PlayLevel,
                SlotId = visitedLevel.SlotId,
                UserId = visitedLevel.UserId,
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
        ActivityEntity? activity = null;
        switch (currentEntity)
        {
            case VisitedLevelEntity visitedLevel:
            {
                if (origEntity is not VisitedLevelEntity) break;

                activity = new LevelActivityEntity
                {
                    Type = EventType.PlayLevel,
                    SlotId = visitedLevel.SlotId,
                    UserId = visitedLevel.UserId,
                };
                break;
            }
            case SlotEntity slotEntity:
            {
                if (origEntity is not SlotEntity oldSlotEntity) break;

                if (!oldSlotEntity.TeamPick && slotEntity.TeamPick)
                {
                    activity = new LevelActivityEntity
                    {
                        Type = EventType.MMPickLevel,
                        SlotId = slotEntity.SlotId,
                        UserId = SlotHelper.GetPlaceholderUserId(database).Result,
                    };
                }
                else if (oldSlotEntity.SlotId == slotEntity.SlotId && slotEntity.Type == SlotType.User)
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

        InsertActivity(database, activity);
    }

    public void OnEntityDeleted<T>(DatabaseContext database, T entity) where T : class
    {
        Console.WriteLine($@"OnEntityDeleted: {entity.GetType().Name}");
        ActivityEntity? activity = entity switch
        {
            //TODO move this to EntityModified and use CommentEntity.Deleted
            CommentEntity comment => comment.Type switch
            {
                CommentType.Level => new CommentActivityEntity
                {
                    Type = EventType.DeleteLevelComment,
                    CommentId = comment.CommentId,
                    UserId = comment.PosterUserId,
                },
                _ => null,
            },
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