#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public class PhotoEntity
{
    [Key]
    public int PhotoId { get; set; }

    // Uses seconds instead of milliseconds for some reason
    public long Timestamp { get; set; }

    public string SmallHash { get; set; } = "";

    public string MediumHash { get; set; } = "";

    public string LargeHash { get; set; } = "";

    public string PlanHash { get; set; } = "";

    public virtual ICollection<PhotoSubjectEntity> PhotoSubjects { get; set; } = new HashSet<PhotoSubjectEntity>();

    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public UserEntity? Creator { get; set; }

    public int? SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity? Slot { get; set; }
}