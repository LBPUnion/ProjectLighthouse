namespace LBPUnion.ProjectLighthouse.Administration;

// Next available ID for use: 18
// PLEASE UPDATE THIS WHEN YOU ADD SOMETHING HERE!
// IF YOU DO NOT ADD THIS IN ORDER PROPERLY THEN THERE WILL BE DATA CORRUPTION!
// THE VALUE MUST ALWAYS BE EXPLICITLY SET.
public enum CaseType
{
    UserSilence = 0,
    UserRestriction = 1,
    UserBan = 2,
    UserDeletion = 3,
    UserCommentsDisabled = 4,
    UserDetailsEdited = 5,
    UserEarthDeletion = 6,
    
    LevelDeletion = 7,
    LevelLock = 8,
    LevelCommentsDisabled = 9,
    LevelDetailsEdited = 10,
    LevelTeamPickAdded = 16,
    LevelTeamPickRemoved = 17,
    
    CommentDeletion = 11,
    ReviewDeletion = 12,
    PhotoDeletion = 13,
    
    HashModeration = 14,
    
    IpAddressBan = 15,
}

public static class CaseTypeExtensions
{
    public static bool AffectsUser(this CaseType type)
    {
        return type switch
        {
            CaseType.UserSilence => true,
            CaseType.UserRestriction => true,
            CaseType.UserBan => true,
            CaseType.UserDeletion => true,
            CaseType.UserCommentsDisabled => true,
            CaseType.UserDetailsEdited => true,
            CaseType.UserEarthDeletion => true,
            _ => false,
        };
    }
    
    public static bool AffectsLevel(this CaseType type)
    {
        return type switch
        {
            CaseType.LevelDeletion => true,
            CaseType.LevelLock => true,
            CaseType.LevelCommentsDisabled => true,
            CaseType.LevelDetailsEdited => true,
            CaseType.LevelTeamPickAdded => true,
            CaseType.LevelTeamPickRemoved => true,
            _ => false,
        };
    }
}