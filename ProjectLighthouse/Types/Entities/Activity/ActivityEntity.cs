using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

public class ActivityEntity
{
    [Key]
    public int ActivityId { get; set; }

    /// <summary>
    /// The time that this event took place.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The <see cref="UserEntity.UserId"/> of the <see cref="UserEntity"/> that triggered this event.
    /// </summary>
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    /// <summary>
    /// The type of this event.
    /// </summary>
    public EventType Type { get; set; }
}