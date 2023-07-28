using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: DpadRateLevel, ReviewLevel, RateLevel, TagLevel
/// </summary>
public class ReviewActivityEntity : ActivityEntity
{
    public int ReviewId { get; set; }

    [ForeignKey(nameof(ReviewId))]
    public ReviewEntity Review { get; set; }
}