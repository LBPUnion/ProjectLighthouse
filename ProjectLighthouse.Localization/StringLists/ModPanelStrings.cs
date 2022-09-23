namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ModPanelStrings
{
    public static readonly TranslatableString ModPanelTitle = create("mod_panel_title");
    public static readonly TranslatableString Greeting = create("greeting");
    public static readonly TranslatableString BannedUsers = create("banned_users");
    public static readonly TranslatableString HiddenLevels = create("hidden_levels");
    
    private static TranslatableString create(string key) => new(TranslationAreas.ModPanel, key);
}