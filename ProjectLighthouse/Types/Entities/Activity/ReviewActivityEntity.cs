using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.ReviewLevel"/>.
/// </summary>
public class ReviewActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="ReviewEntity.ReviewId"/> of the <see cref="ReviewEntity"/> that this event refers to.
    /// </summary>
    public int ReviewId { get; set; }

    [ForeignKey(nameof(ReviewId))]
    public ReviewEntity Review { get; set; }

    [Column("SlotId")]
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}