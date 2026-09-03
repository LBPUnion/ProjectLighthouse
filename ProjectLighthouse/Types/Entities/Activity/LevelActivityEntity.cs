using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.PlayLevel"/>, <see cref="EventType.HeartLevel"/>, <see cref="EventType.PublishLevel"/>,
/// <see cref="EventType.UnheartLevel"/>, <see cref="EventType.DpadRateLevel"/>, <see cref="EventType.RateLevel"/>, <see cref="EventType.TagLevel"/>,
/// and <see cref="EventType.MMPickLevel"/>.
/// <para><see cref="ActivityEntity.Data"/> field usages:</para>
/// <para><see cref="EventType.PublishLevel"/>: Stores if level is republish</para>
/// <para><see cref="EventType.PlayLevel"/>: Stores how many times user played level</para>
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