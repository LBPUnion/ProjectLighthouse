namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class ProfileStrings
{
    public static readonly TranslatableString Title = create("title");
    public static readonly TranslatableString Biography = create("biography");
    public static readonly TranslatableString NoBiography = create("no_biography");

    private static TranslatableString create(string key) => new(TranslationAreas.Profile, key);
}