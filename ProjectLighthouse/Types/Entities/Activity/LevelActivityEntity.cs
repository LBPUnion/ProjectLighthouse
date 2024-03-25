using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.PlayLevel"/>, <see cref="EventType.HeartLevel"/>, <see cref="EventType.PublishLevel"/>,
/// <see cref="EventType.UnheartLevel"/>, and <see cref="EventType.MMPickLevel"/>.
/// </summary>
public class LevelActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="SlotEntity.SlotId"/> of the <see cref="SlotEntity"/> that this event refers to.
    /// </summary>
    [Column("SlotId")]
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}