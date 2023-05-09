using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public struct ApiUser
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public bool EmailAddressVerified { get; set; }
    public string IconHash { get; set; }
    public string Biography { get; set; }
    public Location Location { get; set; }
    public string YayHash { get; set; }
    public string MehHash { get; set; }
    public string BooHash { get; set; }
    public long LastLogin { get; set; }
    public long LastLogout { get; set; }
    public PrivacyType LevelVisibility { get; set; }
    public PrivacyType ProfileVisibility { get; set; }
    public bool CommentsEnabled { get; set; }
    public PermissionLevel PermissionLevel { get; set; }

    public static ApiUser CreateFromEntity(UserEntity entity) =>
        new()
        {
            UserId = entity.UserId,
            Username = entity.Username,
            EmailAddressVerified = entity.EmailAddressVerified,
            IconHash = entity.IconHash,
            Biography = entity.Biography,
            Location = entity.Location,
            YayHash = entity.YayHash,
            MehHash = entity.MehHash,
            BooHash = entity.BooHash,
            LastLogin = entity.LastLogin,
            LastLogout = entity.LastLogin,
            LevelVisibility = entity.LevelVisibility,
            ProfileVisibility = entity.ProfileVisibility,
            CommentsEnabled = entity.CommentsEnabled,
            PermissionLevel = entity.PermissionLevel,
        };
}