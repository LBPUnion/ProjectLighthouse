using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static class FormattingExtensions
{
    public static string GetLevelLockIcon(this SlotEntity slot) => slot.InitiallyLocked ? "ui icon lock" : "";

    public static string GetCaseTypeIcon(this CaseType caseType)
    {
        return caseType switch
        {
            CaseType.UserBan => "ui icon ban",
            CaseType.UserRestriction => "ui icon user alternate slash",
            CaseType.UserSilence => "ui icon volume off",
            CaseType.UserDisableComments => "ui icon comment slash",
            CaseType.LevelHide => "ui icon eye slash",
            CaseType.LevelLock => "ui icon lock",
            CaseType.LevelDisableComments => "ui icon comment slash",
            _ => "ui icon question",
        };
    }

    public static string ToHtmlColor(this PermissionLevel permissionLevel)
    {
        return permissionLevel switch
        {
            PermissionLevel.Administrator => "red",
            PermissionLevel.Moderator => "orange",
            _ => "",
        };
    }
}