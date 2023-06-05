namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ErrorStrings
{
    public static readonly TranslatableString UsernameInvalid = create("username_invalid");
    public static readonly TranslatableString UsernameTaken = create("username_taken");
    public static readonly TranslatableString PasswordInvalid = create("password_invalid");
    public static readonly TranslatableString PasswordDoesntMatch = create("password_doesnt_match");
    public static readonly TranslatableString EmailInvalid = create("email_invalid");
    public static readonly TranslatableString EmailTaken = create("email_taken");
    public static readonly TranslatableString CaptchaFailed = create("captcha_failed");
    public static readonly TranslatableString ActionNoPermission = create("action_no_permission");
    
    private static TranslatableString create(string key) => new(TranslationAreas.Error, key);
}