#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public class LastContactEntity
{
    [Key]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity? User { get; set; }

    public long Timestamp { get; set; }

    public GameVersion GameVersion { get; set; } = GameVersion.Unknown;

    public Platform Platform { get; set; } = Platform.Unknown;
}