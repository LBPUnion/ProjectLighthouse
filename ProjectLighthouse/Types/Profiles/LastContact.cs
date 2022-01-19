using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

public class LastContact
{
    [Key]
    public int UserId { get; set; }

    public long Timestamp { get; set; }

    public GameVersion GameVersion { get; set; } = GameVersion.Unknown;
}