namespace LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

public class UserGeneratedContentLimitConfiguration
{
    /// <summary>
    ///     The maximum amount of slots allowed on users' earth
    /// </summary>
    public int EntitledSlots { get; set; } = 50;

    public int ListsQuota { get; set; } = 50;

    public int PhotosQuota { get; set; } = 500;

    public bool ProfileCommentsEnabled { get; set; } = true;

    public bool LevelCommentsEnabled { get; set; } = true;

    public bool LevelReviewsEnabled { get; set; } = true;

    public bool BooingEnabled { get; set; } = true;
}