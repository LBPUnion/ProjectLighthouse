using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Extensions;

namespace LBPUnion.ProjectLighthouse.Administration;

// Next available ID for use: 6
// PLEASE UPDATE THIS WHEN YOU ADD SOMETHING HERE!
// IF YOU DO NOT ADD THIS IN ORDER PROPERLY THEN THERE WILL BE DATA CORRUPTION!
// THE VALUE MUST ALWAYS BE EXPLICITLY SET.
public enum CaseType
{
    UserSilence = 0,
    UserRestriction = 1,
    UserBan = 2,
    UserDisableComments = 3,
    
    LevelHide = 4,
    LevelDisableComments = 5,
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
            CaseType.UserDisableComments => true,
            _ => false,
        };
    }
    
    public static bool AffectsLevel(this CaseType type)
    {
        return type switch
        {
            CaseType.LevelHide => true,
            CaseType.LevelDisableComments => true,
            _ => false,
        };
    }

    public static async Task<bool> IsIdValid(this CaseType type, int affectedId, Database database)
    {
        if (type.AffectsUser()) return await database.Users.Has(u => u.UserId == affectedId);
        if (type.AffectsLevel()) return await database.Slots.Has(u => u.SlotId == affectedId);

        throw new ArgumentOutOfRangeException();
    }
}