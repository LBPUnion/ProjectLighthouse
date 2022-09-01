namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class StatusStrings
{
    public static readonly TranslatableString CurrentlyOnline = create("currently_online");
    public static readonly TranslatableString LastOnline = create("last_online");
    public static readonly TranslatableString Offline = create("offline");

    private static TranslatableString create(string key) => new(TranslationAreas.Status, key);
}