using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static class FormattingExtensions
{
    public static string GetLevelLockIcon(this SlotEntity slot)
    {
        return slot switch
        {
            { InitiallyLocked: true } => "icon lock",
            _ => ""
        };
    }

    public static string ToHtmlColor(this PermissionLevel permissionLevel)
    {
        return permissionLevel switch
        {
            PermissionLevel.Administrator => "red",
            PermissionLevel.Moderator => "rgb(200, 130, 0)",
            _ => "",
        };
    }
}