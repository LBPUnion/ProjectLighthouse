using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

public class LastContact
{
    [Key]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public long Timestamp { get; set; }

    public GameVersion GameVersion { get; set; } = GameVersion.Unknown;

    public Platform Platform { get; set; } = Platform.Unknown;
}