using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

[PrimaryKey(nameof(UserId), nameof(GameVersion))]
public class UserProfilePinsEntity
{
    public int UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public GameVersion GameVersion { get; set; }
    public string Pins { get; set; } = "";
}
