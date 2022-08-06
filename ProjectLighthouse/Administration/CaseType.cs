namespace LBPUnion.ProjectLighthouse.Administration;

// Next available ID for use: 7
// PLEASE UPDATE THIS WHEN YOU ADD SOMETHING HERE!
// IF YOU DO NOT ADD THIS IN ORDER PROPERLY THEN THERE WILL BE DATA CORRUPTION!
// THE VALUE MUST ALWAYS BE EXPLICITLY SET.
public enum CaseType
{
    UserSilence = 0,
    UserRestriction = 1,
    UserBan = 2,
    UserCommentsDisabled = 3,
    
    LevelLock = 4,
    LevelCommentsDisabled = 5,
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
            CaseType.UserCommentsDisabled => true,
            _ => false,
        };
    }
    
    public static bool AffectsLevel(this CaseType type)
    {
        return type switch
        {
            CaseType.LevelLock => true,
            CaseType.LevelCommentsDisabled => true,
            _ => false,
        };
    }
}