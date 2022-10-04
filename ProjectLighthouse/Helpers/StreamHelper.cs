namespace LBPUnion.ProjectLighthouse.Helpers;

public enum EventType
{
    News,
    TeamPick,
    LevelInteraction,
    DpadRating,
    Review,
    Score,
    CommentUser,
    UploadPhoto,
    PublishLevel,
    HeartLevel,
    HeartUser,
    PlayLevel,
    Other
}

public static class StreamHelper
{
    // Not entirely sure how enums work in C#, 
    // please let me know if theres a more efficient way of handling this!
    public static EventType convertPost(string PostType) {
        switch (PostType)
        {
            case "news_post":
                return EventType.News;
            case "mm_pick_level":
                return EventType.TeamPick;
            // Subgroups are used past this point
            case "dpad_rate_level":
                return EventType.DpadRating;
            case "review":
                return EventType.Review;
            case "score":
                return EventType.Score;
            case "comment_on_user":
                return EventType.CommentUser;
            case "upload_photo":
                return EventType.UploadPhoto;
            case "publish_level":
                return EventType.PublishLevel;
            case "heart_level":
                return EventType.HeartLevel;
            case "heart_user":
                return EventType.HeartUser;
            case "play_level":
                return EventType.PlayLevel;
            default:
                return EventType.LevelInteraction;
        }
    }
}