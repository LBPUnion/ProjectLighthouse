using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Misc;

public class SlotMetadata
{
    public required SlotEntity Slot { get; init; }
    public double RatingLbp1 { get; init; }
    public int ThumbsUp { get; init; }
    public int Hearts { get; init; }
    public bool Played { get; set; }
}