#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

public class RatedCommentEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int RatingId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity? User { get; set; }

    [ForeignKey(nameof(CommentId))]
    public CommentEntity? Comment { get; set; }

    public int CommentId { get; set; }

    public int Rating { get; set; }

}