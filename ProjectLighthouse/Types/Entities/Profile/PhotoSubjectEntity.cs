using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public class PhotoSubjectEntity
{
    [Key]
    public int PhotoSubjectId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int PhotoId { get; set; }

    [ForeignKey(nameof(PhotoId))]
    public PhotoEntity Photo { get; set; }

    public string Bounds { get; set; }
}