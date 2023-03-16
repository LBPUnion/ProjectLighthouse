namespace LBPUnion.ProjectLighthouse.Types.Users;

public enum PermissionLevel
{
    Banned = -3,
    Restricted = -2,
    Silenced = -1,
    Default = 0,
    Moderator = 1,
    Administrator = 2,
}

public static class PermissionLevelExtensions
{
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