using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public class RatedLevel
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int RatedLevelId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public Slot Slot { get; set; }

    public int Rating { get; set; }

    public double RatingLBP1 { get; set; }
}