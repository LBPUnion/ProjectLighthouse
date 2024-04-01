namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class UserGeneratedContentLimitConfiguration
{
    /// <summary>
    ///     The maximum amount of slots allowed on users' earth
    /// </summary>
    public int EntitledSlots { get; set; } = 50;

    public int ListsQuota { get; set; } = 50;

    public int PhotosQuota { get; set; } = 500;

    /// <summary>
    ///     When enabled, all UGC uploads are disabled. This includes levels, photos, reviews,
    ///     comments, and certain profile settings.
    /// </summary>
    public bool ReadOnlyMode { get; set; } = false;

    public bool ProfileCommentsEnabled { get; set; } = true;

    public bool LevelCommentsEnabled { get; set; } = true;

    public bool LevelReviewsEnabled { get; set; } = true;

    public bool BooingEnabled { get; set; } = true;
}