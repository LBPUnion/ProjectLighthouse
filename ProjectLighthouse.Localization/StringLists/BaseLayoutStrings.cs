namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class BaseLayoutStrings
{
    public static readonly TranslatableString HeaderHome = create("header_home");
    public static readonly TranslatableString HeaderUsers = create("header_users");
    public static readonly TranslatableString HeaderPhotos = create("header_photos");
    public static readonly TranslatableString HeaderSlots = create("header_slots");
    public static readonly TranslatableString HeaderAuthentication = create("header_authentication");

    public static readonly TranslatableString HeaderLoginRegister = create("header_loginRegister");
    public static readonly TranslatableString HeaderProfile = create("header_profile");
    public static readonly TranslatableString HeaderAdminPanel = create("header_adminPanel");
    public static readonly TranslatableString HeaderLogout = create("header_logout");

    private static TranslatableString create(string key) => new(TranslationAreas.BaseLayout, key);
}