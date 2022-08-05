namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class RegisterStrings
{
    public static readonly TranslatableString UsernameNotice = create("username_notice");
    
    private static TranslatableString create(string key) => new(TranslationAreas.Register, key);
}