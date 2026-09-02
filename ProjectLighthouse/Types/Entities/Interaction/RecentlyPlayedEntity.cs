#nullable enable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

[Index(nameof(UserId), nameof(SlotId), IsUnique = true)]
public class RecentlyPlayedEntity
{
    [Key]
    public int RecentlyPlayedId { get; set; }
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; } = null!;
    public long LastPlayedAt { get; set; }
}
