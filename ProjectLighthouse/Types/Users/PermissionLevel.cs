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