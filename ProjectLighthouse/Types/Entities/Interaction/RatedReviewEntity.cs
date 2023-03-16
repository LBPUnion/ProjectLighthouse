using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

public class RatedReviewEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int RatedReviewId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int ReviewId { get; set; }

    [ForeignKey(nameof(ReviewId))]
    public ReviewEntity Review { get; set; }

    public int Thumb { get; set; }
}