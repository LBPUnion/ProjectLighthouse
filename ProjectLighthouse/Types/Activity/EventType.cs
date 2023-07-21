using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Activity;

public enum EventType
{
    [XmlEnum("heart_level")]
    HeartLevel,

    [XmlEnum("unheart_level")]
    UnheartLevel,

    [XmlEnum("heart_user")]
    HeartUser,

    [XmlEnum("unheart_user")]
    UnheartUser,

    [XmlEnum("play_level")]
    PlayLevel,

    [XmlEnum("rate_level")]
    RateLevel,

    [XmlEnum("tag_level")]
    TagLevel,

    [XmlEnum("comment_on_level")]
    CommentOnLevel,

    [XmlEnum("delete_level_comment")]
    DeleteLevelComment,

    [XmlEnum("upload_photo")]
    UploadPhoto,

    [XmlEnum("publish_level")]
    PublishLevel,

    [XmlEnum("unpublish_level")]
    UnpublishLevel,

    [XmlEnum("score")]
    Score,

    [XmlEnum("news_post")]
    NewsPost,

    [XmlEnum("mm_pick_level")]
    MMPickLevel,

    [XmlEnum("dpad_rate_level")]
    DpadRateLevel,

    [XmlEnum("review_level")]
    ReviewLevel,

    [XmlEnum("comment_on_user")]
    CommentOnUser,

    [XmlEnum("create_playlist")]
    CreatePlaylist,

    [XmlEnum("heart_playlist")]
    HeartPlaylist,

    [XmlEnum("add_level_to_playlist")]
    AddLevelToPlaylist,
}