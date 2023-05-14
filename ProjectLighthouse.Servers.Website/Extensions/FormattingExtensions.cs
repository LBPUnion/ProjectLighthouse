using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static class FormattingExtensions
{
    public static string GetLevelLockIcon(this SlotEntity slot) => slot.InitiallyLocked ? "ui white icon lock" : "";
    public static string GetTeamPickedIcon(this SlotEntity slot) => slot.TeamPick ? "ui pink icon certificate" : "";
    
    // ReSharper disable once UnusedParameter.Global
    public static string GetLevelWarningIcon(this SlotEntity slot) => "ui orange icon exclamation circle";

    // ReSharper disable once ConvertIfStatementToReturnStatement
    // These messages are sorted by logical priority. No two should happen at once.
    public static string GetLevelWarningText(this SlotEntity slot)
    {
        if (slot.Lbp1Only) return "This slot is designed for LittleBigPlanet 1 only.";
        if (slot.CrossControllerRequired) return "This slot is designed to be played in Cross-Controller mode.";
        if (slot.MoveRequired) return "This level requires a PlayStation Move controller.";
        return "";
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