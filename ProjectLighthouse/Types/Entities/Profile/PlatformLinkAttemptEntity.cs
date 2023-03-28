using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public class PlatformLinkAttemptEntity
{
    [Key]
    public int PlatformLinkAttemptId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int UserId { get; set; }

    public ulong PlatformId { get; set; }

    public Platform Platform { get; set; }

    public long Timestamp { get; set; }

    public string IPAddress { get; set; } = "";
}