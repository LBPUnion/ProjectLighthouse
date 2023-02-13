using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public class BlockedProfile
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int BlockedProfileId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int BlockedUserId { get; set; }

    [ForeignKey(nameof(BlockedUserId))]
    public User BlockedUser { get; set; }
}