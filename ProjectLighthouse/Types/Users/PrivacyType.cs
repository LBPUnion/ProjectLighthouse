using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Localization.StringLists;

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
    public static TranslatableString ToReadableString(this PrivacyType type)
    {
        return type switch
        {
            PrivacyType.All => PrivacyStrings.PrivacyAll,
            PrivacyType.PSN => PrivacyStrings.PrivacyPSN,
            PrivacyType.Game => PrivacyStrings.PrivacyGame,
            _ => null,
        };
    }
    
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

    public static bool CanAccess(this PrivacyType type, bool authenticated, bool owner)
    {
        return type switch
        {
            PrivacyType.All => true,
            PrivacyType.PSN => authenticated,
            PrivacyType.Game => authenticated && owner,
            _ => false,
        };
    }
}