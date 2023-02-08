using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.PlayerData.Profiles;

public class BlockedProfile
{
    [Key]
    public int BlockedProfileId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int BlockedUserId { get; set; }

    [ForeignKey(nameof(BlockedUserId))]
    public User BlockedUser { get; set; }
}