#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Users;

namespace LBPUnion.ProjectLighthouse.Entities.Profile;

public class LastContact
{
    [Key]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public long Timestamp { get; set; }

    public GameVersion GameVersion { get; set; } = GameVersion.Unknown;

    public Platform Platform { get; set; } = Platform.Unknown;
}