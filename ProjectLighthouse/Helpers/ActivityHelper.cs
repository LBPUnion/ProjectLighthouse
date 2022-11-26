using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public enum TargetType
{
    News,
    LBP3TeamPick,
    Level,
    Profile
}

public enum EventType
{
    Public,
    PublishLevel,
    PlayLevel,
    Score,
    UploadPhoto,
    LBP1Rate,
    DpadRating,
    Review,
    CommentLevel,
    HeartLevel,
    CommentUser,
    HeartUser,
}

#nullable enable

public class ActivityHelper
{
    public static string ObjectAsEventElement(TargetType targetType, int targetId, Database database)
    {
        switch (targetType)
        {
            default:
            case TargetType.Level:
            case TargetType.LBP3TeamPick:
                return LbpSerializer.TaggedStringElement("object_slot_id", targetId, "type", "user");
            case TargetType.Profile:
                User? user = database.Users.Include(u => u.Location).FirstOrDefault(u => u.UserId == targetId);
                return LbpSerializer.StringElement("object_user", user?.Username);
            case TargetType.News:
                return LbpSerializer.StringElement("news_id", targetId);
        }
    }

    public static string ObjectAsGroupElement(TargetType targetType, int targetId, Database database)
    {
        switch (targetType)
        {
            default:
            case TargetType.Level:
            case TargetType.LBP3TeamPick:
                return LbpSerializer.TaggedStringElement("slot_id", targetId, "type", "user");
            case TargetType.Profile:
                User? user = database.Users.Include(u => u.Location).FirstOrDefault(u => u.UserId == targetId);
                return LbpSerializer.StringElement("user_id", user?.Username);
            case TargetType.News:
                return LbpSerializer.StringElement("news_id", targetId);
        }
    }


    public static string EventTypeAsString(EventType actionType)
    {
        switch(actionType)
        {
            case EventType.DpadRating:
                return "dpad_rate_level";
            case EventType.LBP1Rate:
                return "rate_level";
            case EventType.Review:
                return "review_level";
            case EventType.Score:
                return "score";
            case EventType.CommentUser:
                return "comment_on_user";
            case EventType.CommentLevel:
                return "comment_on_level";
            case EventType.UploadPhoto:
                return "upload_photo";
            case EventType.PublishLevel:
                return "publish_level";
            case EventType.HeartLevel:
                return "heart_level";
            case EventType.HeartUser:
                return "heart_user";
            case EventType.PlayLevel:
                return "play_level";
            default: return "invalid_event";
        }
    }
    public static async Task<object?> ObjectFinder(Database database, TargetType targetType, int targetId)
    {
        switch (targetType)
        {
            default:
            case TargetType.Level:
            case TargetType.LBP3TeamPick:
                Slot? slot = await database.Slots.Include(s => s.Creator).FirstOrDefaultAsync(s => s.SlotId == targetId);
                if (slot?.Creator == null) return null;
                return slot;
            case TargetType.Profile:
                User? user = await database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == targetId);
                return user;
            case TargetType.News:
                News? news = await database.News.FirstOrDefaultAsync(n => n.NewsId == targetId);
                return news;
        }
    }
}