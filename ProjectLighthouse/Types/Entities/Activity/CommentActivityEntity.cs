using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.CommentOnUser"/>, <see cref="EventType.CommentOnLevel"/>, and <see cref="EventType.DeleteLevelComment"/>.
/// </summary>
public class CommentActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="CommentEntity.CommentId"/> of the <see cref="CommentEntity"/> that this event refers to.
    /// </summary>
    public int CommentId { get; set; }

    [ForeignKey(nameof(CommentId))]
    public CommentEntity Comment { get; set; }
}

public class LevelCommentActivityEntity : CommentActivityEntity
{
    [Column("SlotId")]
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}

public class UserCommentActivityEntity : CommentActivityEntity
{
    [Column("TargetUserId")]
    public int TargetUserId { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public UserEntity TargetUser { get; set; }
}