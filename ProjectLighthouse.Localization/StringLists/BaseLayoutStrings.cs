namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class BaseLayoutStrings
{
    public static readonly TranslatableString HeaderHome = create("header_home");
    public static readonly TranslatableString HeaderUsers = create("header_users");
    public static readonly TranslatableString HeaderPhotos = create("header_photos");
    public static readonly TranslatableString HeaderSlots = create("header_slots");

    private static TranslatableString create(string key) => new(TranslationAreas.BaseLayout, key);
}