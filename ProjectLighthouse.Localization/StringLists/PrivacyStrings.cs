namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class PrivacyStrings
{
    public static readonly TranslatableString BlockedUsers = create("blocked_users");
    public static readonly TranslatableString NoBlockedUsers = create("no_blocked_users");
    
    public static readonly TranslatableString EnableComments = create("enable_comments");
    public static readonly TranslatableString DisableComments = create("disable_comments");
    
    public static readonly TranslatableString PrivacyAll = create("privacy_all");
    public static readonly TranslatableString PrivacyPSN = create("privacy_psn");
    public static readonly TranslatableString PrivacyGame = create("privacy_game");
    
    private static TranslatableString create(string key) => new(TranslationAreas.Privacy, key);
}