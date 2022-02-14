using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types;

public class Reaction
{
    [Key]
    public int RatingId { get; set; }

    public int UserId { get; set; }

    public int TargetId { get; set; }

    public int Rating { get; set; }

}