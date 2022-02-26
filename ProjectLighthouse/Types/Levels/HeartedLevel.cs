#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public class HeartedLevel
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int HeartedLevelId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int SlotId { get; set; }

    public SlotType SlotType { get; set; }

    public long Timestamp { get; set; }
}