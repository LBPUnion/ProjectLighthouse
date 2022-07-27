namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class GeneralStrings
{
    public static readonly TranslatableString Username = create("username");
    public static readonly TranslatableString Password = create("password");
    public static readonly TranslatableString Register = create("register");
    public static readonly TranslatableString ForgotPassword = create("forgot_password");
    public static readonly TranslatableString Error = create("error");
    public static readonly TranslatableString LogIn = create("log_in");
    
    private static TranslatableString create(string key) => new(TranslationAreas.General, key);
}