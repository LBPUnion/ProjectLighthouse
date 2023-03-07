using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

public class HeartedProfileEntity
{
    [Key]
    public int HeartedProfileId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int HeartedUserId { get; set; }

    [ForeignKey(nameof(HeartedUserId))]
    public UserEntity HeartedUser { get; set; }
}