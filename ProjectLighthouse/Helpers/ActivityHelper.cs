using System;
using System.ComponentModel;

namespace LBPUnion.ProjectLighthouse.Helpers;

public enum EventType
{
    News,
    TeamPick,
    DpadRating,
    LBP1Rate,
    Review,
    Score,
    CommentUser,
    CommentLevel,
    UploadPhoto,
    PublishLevel,
    HeartLevel,
    HeartUser,
    PlayLevel,
    PublishPlaylist
}

public enum ActivityCategory
{
    News = 0,
    TeamPick = 1,
    Level = 2,
    User = 3,
    // UNCOMBINABLES!
    // The following can not be combined with any other event, 
    // and in rare cases, themselves.
    Comment = 4,
    HeartUser = 5,
    UserComment = 6,
}

public static class ActivityHelper
{
    public static string ObjectType(int action)
    {
        switch ((EventType)action)
        {
            case EventType.News:
                return "news_id";
            case EventType.TeamPick:
            case EventType.DpadRating:
            case EventType.Review:
            case EventType.Score:
            case EventType.UploadPhoto:
            case EventType.PublishLevel:
            case EventType.HeartLevel:
            case EventType.PlayLevel:
            default:
                return "object_slot_id";
            case EventType.CommentUser:
            case EventType.HeartUser:
                return "object_user";
            case EventType.PublishPlaylist:
                return "object_playlist_id";
        }
    }
    
    public static string EventTypeAsString(int actionType)
    {
        switch((EventType)actionType)
        {
            case EventType.News:
                return "news_post";
            case EventType.TeamPick:
                return "mm_pick_level";
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
}