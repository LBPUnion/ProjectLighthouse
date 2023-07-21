using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: CommentOnUser, CommentOnLevel, DeleteLevelComment
/// </summary>
public class CommentActivityEntity : ActivityEntity
{
    public int CommentId { get; set; }

    [ForeignKey(nameof(CommentId))]
    public CommentEntity Comment { get; set; }
}