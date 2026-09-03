using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.HeartUser"/> and <see cref="EventType.UnheartUser"/>.
/// </summary>
public class UserActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="UserEntity.UserId"/> of the <see cref="UserEntity"/> that this event refers to.
    /// </summary>
    [Column("TargetUserId")]
    public int TargetUserId { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public UserEntity TargetUser { get; set; }
}