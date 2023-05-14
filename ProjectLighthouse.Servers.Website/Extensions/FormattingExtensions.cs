using System.Diagnostics.CodeAnalysis;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Extensions;

public static class FormattingExtensions
{
    private static string GetLevelLockIcon(this SlotEntity slot) => slot.InitiallyLocked ? "ui white icon lock" : "";
    private static string GetTeamPickedIcon(this SlotEntity slot) => slot.TeamPick ? "ui pink icon certificate" : "";

    // ReSharper disable once UnusedParameter.Global
    private static string GetLevelWarningIcon
        (this SlotEntity slot) =>
        slot.Lbp1Only || slot.CrossControllerRequired || slot.MoveRequired ? "ui orange icon exclamation circle" : "";

    [SuppressMessage("ReSharper", "ArrangeTrailingCommaInSinglelineLists")]
    // These messages are sorted by logical priority. No two should happen at once.
    private static string GetLevelWarningText(this SlotEntity slot)
    {
        return slot switch
        {
            {
                Lbp1Only: true,
            } => "This level is designed for LittleBigPlanet 1 only.",
            {
                CrossControllerRequired: true,
            } => "This level is designed to be played in Cross-Controller mode.",
            {
                MoveRequired: true,
            } => "This level requires a PlayStation Move controller.",
            _ => "",
        };
    }

    public static string RenderAllIcons(this SlotEntity slot)
    { 
        string GenerateIconHtml(string icon, string? title) => $"<i class=\"{icon}\" title=\"{title}\"></i>";

        return string.Join("\n",
            GenerateIconHtml(slot.GetLevelLockIcon(), null),
            GenerateIconHtml(slot.GetTeamPickedIcon(), null),
            GenerateIconHtml(slot.GetLevelWarningIcon(), slot.GetLevelWarningText()));
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