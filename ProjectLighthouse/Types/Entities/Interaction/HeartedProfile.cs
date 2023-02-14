using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

public class HeartedProfile
{
    [Key]
    public int HeartedProfileId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int HeartedUserId { get; set; }

    [ForeignKey(nameof(HeartedUserId))]
    public User HeartedUser { get; set; }
}