using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public class VisitedLevel
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int VisitedLevelId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public Slot Slot { get; set; }

    public int PlaysLBP1 { get; set; }
    public int PlaysLBP2 { get; set; }
    public int PlaysLBP3 { get; set; }
    public int PlaysLBPVita { get; set; }
}