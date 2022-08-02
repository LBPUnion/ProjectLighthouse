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

    public static readonly TranslatableString GeneratedBy = create("generated_by");
    public static readonly TranslatableString GeneratedModified = create("generated_modified");

    public static readonly TranslatableString JavaScriptWarnTitle = create("js_warn_title");
    public static readonly TranslatableString JavaScriptWarn = create("js_warn");
    public static readonly TranslatableString LicenseWarnTitle = create("license_warn_title");
    public static readonly TranslatableString LicenseWarn1 = create("license_warn_1");
    public static readonly TranslatableString LicenseWarn2 = create("license_warn_2");
    public static readonly TranslatableString LicenseWarn3 = create("license_warn_3");

    private static TranslatableString create(string key) => new(TranslationAreas.BaseLayout, key);
}