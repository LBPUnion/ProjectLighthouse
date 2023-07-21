using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: HeartUser, UnheartUser
/// </summary>
public class UserActivityEntity : ActivityEntity
{
    public int TargetUserId { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public UserEntity TargetUser { get; set; }
}