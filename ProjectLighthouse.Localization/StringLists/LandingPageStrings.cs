namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class LandingPageStrings
{
    public static readonly TranslatableString Welcome = create("welcome");
    public static readonly TranslatableString LoggedInAs = create("loggedInAs");

    public static readonly TranslatableString UsersNone = create("users_none");
    public static readonly TranslatableString UsersSingle = create("users_single");
    public static readonly TranslatableString UsersMultiple = create("users_multiple");

    public static readonly TranslatableString LatestTeamPicks = create("latest_team_picks");
    public static readonly TranslatableString NewestLevels = create("newest_levels");

    public static readonly TranslatableString AuthAttemptsPending = create("authAttemptsPending");

    private static TranslatableString create(string key) => new(TranslationAreas.LandingPage, key);
}