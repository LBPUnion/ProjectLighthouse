using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: Score
/// </summary>
public class ScoreActivityEntity : ActivityEntity
{
    public int ScoreId { get; set; }

    [ForeignKey(nameof(ScoreId))]
    public ScoreEntity Score { get; set; }
}