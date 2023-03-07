#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Level;

public class ReviewEntity
{
    [Key]
    public int ReviewId { get; set; }

    public int ReviewerId { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public UserEntity? Reviewer { get; set; }

    public int SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public SlotEntity? Slot { get; set; }

    public long Timestamp { get; set; }

    public string LabelCollection { get; set; } = "";

    [NotMapped]
    public string[] Labels {
        get => this.LabelCollection.Split(",", StringSplitOptions.RemoveEmptyEntries);
        set => this.LabelCollection = string.Join(',', value);
    }

    public bool Deleted { get; set; }

    public DeletedBy DeletedBy { get; set; }

    public string Text { get; set; } = "";

    public int Thumb { get; set; }

    public int ThumbsUp { get; set; }

    public int ThumbsDown { get; set; }
}