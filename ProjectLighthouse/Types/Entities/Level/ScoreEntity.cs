using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Level;

public class ScoreEntity
{
    [Key]
    public int ScoreId { get; set; }

    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }

    public int ChildSlotId { get; set; }

    public int Type { get; set; }
    
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int Points { get; set; }

    public long Timestamp { get; set; }
}