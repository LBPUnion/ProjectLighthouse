namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ModerationStrings
{
    // Main page strings
    public static readonly TranslatableString SuspensionHeading = create("suspension_heading");
    public static readonly TranslatableString SuspensionExplanation = create("suspension_explanation");
    public static readonly TranslatableString SuspensionExpiration = create("suspension_expiration");
    public static readonly TranslatableString SuspensionReason = create("suspension_reason");
    public static readonly TranslatableString SuspensionCircumventWarning = create("suspension_circumvent_warning");

    // Translatable string in case a ban doesn't expire
    public static readonly TranslatableString DoesNotExpire = create("does_not_expire");

    // Restricted features strings
    public static readonly TranslatableString LbpOnlineMultiplayer = create("lbp_online_multiplayer");
    public static readonly TranslatableString WebsiteInteractions = create("website_interactions");
    public static readonly TranslatableString ProfileVisibility = create("profile_visibility");
    public static readonly TranslatableString AccountProfileManagement = create("account_profile_management");

    private static TranslatableString create(string key) => new(TranslationAreas.Moderation, key);
}