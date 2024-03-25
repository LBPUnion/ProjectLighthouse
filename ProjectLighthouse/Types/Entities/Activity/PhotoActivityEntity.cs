using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.UploadPhoto"/>.
/// </summary>
public class PhotoActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="PhotoEntity.PhotoId"/> of the <see cref="PhotoEntity"/> that this event refers to.
    /// </summary>
    public int PhotoId { get; set; }

    [ForeignKey(nameof(PhotoId))]
    public PhotoEntity Photo { get; set; }
}

public class LevelPhotoActivity : PhotoActivityEntity
{
    [Column("SlotId")]
    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }
}

public class UserPhotoActivity : PhotoActivityEntity
{
    [Column("TargetUserId")]
    public int TargetUserId { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public UserEntity TargetUser { get; set; }
}