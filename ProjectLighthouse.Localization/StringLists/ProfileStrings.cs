namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ProfileStrings
{
    public static readonly TranslatableString Title = create("title");
    public static readonly TranslatableString Biography = create("biography");
    public static readonly TranslatableString NoBiography = create("no_biography");
    public static readonly TranslatableString ProfileTag = create("profile_tag");
    public static readonly TranslatableString Playlists = create("playlists");
    public static readonly TranslatableString HeartedLevels = create("hearted_levels");
    public static readonly TranslatableString QueuedLevels = create("queued_levels");
    public static readonly TranslatableString ProfileSettings = create("profile_settings");
    public static readonly TranslatableString PrivacySettings = create("privacy_settings");
    public static readonly TranslatableString Block = create("block");
    public static readonly TranslatableString Unblock = create("unblock");
    public static readonly TranslatableString UserBanned = create("user_banned");
    public static readonly TranslatableString BanReason = create("ban_reason");
    public static readonly TranslatableString BanReasonTOS = create("ban_reason_tos");
    public static readonly TranslatableString PrivacyHidden = create("privacy_hidden");
    public static readonly TranslatableString CommentsBanned = create("comments_banned");
    public static readonly TranslatableString NoPhotos = create("no_photos");
    public static readonly TranslatableString NoLevels = create("no_levels");
    public static readonly TranslatableString NoHeartedLevels = create("no_hearted_levels");
    public static readonly TranslatableString HeartedLevelsCount = create("hearted_levels_count");
    public static readonly TranslatableString NoQueuedLevels = create("no_queued_levels");
    public static readonly TranslatableString QueuedLevelsCount = create("queued_levels_count");

    private static TranslatableString create(string key) => new(TranslationAreas.Profile, key);
}