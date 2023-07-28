using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: play_level, heart_level, publish_level, unheart_level, dpad_rate_level, rate_level, tag_level, mm_pick_level  
/// </summary>
public class LevelActivityEntity : ActivityEntity
{
    [Column("SlotId")]
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}