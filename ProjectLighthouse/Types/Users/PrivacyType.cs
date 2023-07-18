using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

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
    public static string ToReadableString(this PrivacyType type, string area)
    {
        return type switch
        {
            PrivacyType.All => $"Share your {area} with everyone!",
            PrivacyType.PSN => $"Only share your {area} with users who are signed into the website or playing in-game.",
            PrivacyType.Game => $"Only share your {area} with users who are playing in-game.",
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
    
    public static bool IsPrivate(this PrivacyType type, UserEntity user)
    {
        return type switch                                   
        {
            PrivacyType.All => false,
            PrivacyType.PSN => user == null || !user.IsModerator, 
            PrivacyType.Game => !user.IsModerator,
            _ => false,
        };
    }
}