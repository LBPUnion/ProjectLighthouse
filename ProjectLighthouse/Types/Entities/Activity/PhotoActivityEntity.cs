using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: UploadPhoto
/// </summary>
public class PhotoActivityEntity : ActivityEntity
{
    public int PhotoId { get; set; }

    [ForeignKey(nameof(PhotoId))]
    public PhotoEntity Photo { get; set; }
    
}