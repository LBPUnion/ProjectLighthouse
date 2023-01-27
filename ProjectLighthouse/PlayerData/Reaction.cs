using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.PlayerData;

public class Reaction
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int RatingId { get; set; }

    public int UserId { get; set; }

    public int TargetId { get; set; }

    public int Rating { get; set; }

}