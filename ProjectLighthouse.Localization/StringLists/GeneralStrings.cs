namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class GeneralStrings
{
    public static readonly TranslatableString Username = create("username");
    public static readonly TranslatableString Password = create("password");
    public static readonly TranslatableString Email = create("email");
    public static readonly TranslatableString Register = create("register");
    public static readonly TranslatableString ResetPassword = create("reset_password");
    public static readonly TranslatableString ForgotPassword = create("forgot_password");
    public static readonly TranslatableString Success = create("success");
    public static readonly TranslatableString Error = create("error");
    public static readonly TranslatableString LogIn = create("log_in");
    public static readonly TranslatableString Unknown = create("unknown");
    public static readonly TranslatableString RecentPhotos = create("recent_photos");
    public static readonly TranslatableString RecentActivity = create("recent_activity");
    public static readonly TranslatableString Soon = create("soon");
    
    private static TranslatableString create(string key) => new(TranslationAreas.General, key);
}