using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

public class QueuedLevelEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int QueuedLevelId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}