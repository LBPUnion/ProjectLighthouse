namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ModerationStrings
{
    public static readonly TranslatableString BanHeading = create("ban_heading");
    public static readonly TranslatableString BanExplanation = create("ban_explanation");
    public static readonly TranslatableString BanReason = create("ban_reason");
    public static readonly TranslatableString BanIssued = create("ban_issued");
    public static readonly TranslatableString BanExpires = create("ban_expires");
    public static readonly TranslatableString BanDoesNotExpire = create("ban_does_not_expire");

    private static TranslatableString create(string key) => new(TranslationAreas.Moderation, key);
}