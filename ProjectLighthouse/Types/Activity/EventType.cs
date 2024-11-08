using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Activity;

/// <summary>
/// An enum of all possible event types that LBP recognizes in Recent Activity
/// <remarks>
/// <para>
/// <see cref="UnheartLevel"/>, <see cref="UnheartUser"/>, <see cref="DeleteLevelComment"/>, <see cref="UnpublishLevel"/> are ignored by the game
/// </para>
/// </remarks>
/// </summary>
public enum EventType
{
    [XmlEnum("heart_level")]
    HeartLevel = 0,

    [XmlEnum("unheart_level")]
    UnheartLevel = 1,

    [XmlEnum("heart_user")]
    HeartUser = 2,

    [XmlEnum("unheart_user")]
    UnheartUser = 3,

    [XmlEnum("play_level")]
    PlayLevel = 4,

    [XmlEnum("rate_level")]
    RateLevel = 5,

    [XmlEnum("tag_level")]
    TagLevel = 6,

    [XmlEnum("comment_on_level")]
    CommentOnLevel = 7,

    [XmlEnum("delete_level_comment")]
    DeleteLevelComment = 8,

    [XmlEnum("upload_photo")]
    UploadPhoto = 9,

    [XmlEnum("publish_level")]
    PublishLevel = 10,

    [XmlEnum("unpublish_level")]
    UnpublishLevel = 11,

    [XmlEnum("score")]
    Score = 12,

    [XmlEnum("news_post")]
    NewsPost = 13,

    [XmlEnum("mm_pick_level")]
    MMPickLevel = 14,

    [XmlEnum("dpad_rate_level")]
    DpadRateLevel = 15,

    [XmlEnum("review_level")]
    ReviewLevel = 16,

    [XmlEnum("comment_on_user")]
    CommentOnUser = 17,

    /// <remarks>
    /// This event is only used in LBP3
    /// </remarks>>
    [XmlEnum("create_playlist")]
    CreatePlaylist = 18,

    /// <remarks>
    /// This event is only used in LBP3
    /// </remarks>>
    [XmlEnum("heart_playlist")]
    HeartPlaylist = 19,

    /// <remarks>
    /// This event is only used in LBP3
    /// </remarks>>
    [XmlEnum("add_level_to_playlist")]
    AddLevelToPlaylist = 20,
}