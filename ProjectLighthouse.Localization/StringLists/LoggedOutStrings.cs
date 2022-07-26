namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class LoggedOutStrings
{
    public static readonly TranslatableString LoggedOut = create("logged_out");
    public static readonly TranslatableString LoggedOutInfo = create("logged_out_info");
    public static readonly TranslatableString Redirect = create("redirect");
    
    private static TranslatableString create(string key) => new(TranslationAreas.LoggedOut, key);
}