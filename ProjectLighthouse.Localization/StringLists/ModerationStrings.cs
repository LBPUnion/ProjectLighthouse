namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ModerationStrings
{
    public static readonly TranslatableString BannedHeading = create("banned_heading");
    public static readonly TranslatableString BannedExplain = create("banned_explain");

    public static readonly TranslatableString BanReason = create("ban_reason");

    public static readonly TranslatableString CaseCreated = create("case_created");
    public static readonly TranslatableString CaseExpires = create("case_expires");

    public static readonly TranslatableString AppealInstructions = create("appeal_instructions");

    private static TranslatableString create(string key) => new(TranslationAreas.Moderation, key);
}