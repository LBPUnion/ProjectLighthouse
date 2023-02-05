namespace LBPUnion.ProjectLighthouse.Types.Users;

/// <summary>
/// Where user levels/profiles should show.
/// </summary>
public enum PrivacyType
{
    /// <summary>
    /// Shows your levels/profile only to those signed in on the website or the game.
    /// </summary>
    PSN = 0,
    /// <summary>
    /// Shows your levels/profile only to those in-game.
    /// </summary>
    Game = 1,
    /// <summary>
    /// Shows your levels/profile to everyone.
    /// </summary>
    All = 2,
}

public static class PrivacyTypeExtensions
{
    public static string ToSerializedString(this PrivacyType type) 
        => type.ToString().ToLower();

    public static PrivacyType? FromSerializedString(string type)
    {
        return type switch
        {
            "psn" => PrivacyType.PSN,
            "game" => PrivacyType.Game,
            "all" => PrivacyType.All,
            _ => null,
        };
    }
}