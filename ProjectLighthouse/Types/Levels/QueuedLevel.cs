using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public class QueuedLevel
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int QueuedLevelId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int SlotId { get; set; }

    public long Timestamp { get; set; }

    [ForeignKey(nameof(SlotId))]
    public Slot Slot { get; set; }
}