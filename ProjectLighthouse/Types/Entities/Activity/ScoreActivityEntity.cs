using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.Score"/>.
/// </summary>
public class ScoreActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="ScoreEntity.ScoreId"/> of the <see cref="ScoreEntity"/> that this event refers to.
    /// </summary>
    public int ScoreId { get; set; }

    [ForeignKey(nameof(ScoreId))]
    public ScoreEntity Score { get; set; }

    [Column("SlotId")]
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}