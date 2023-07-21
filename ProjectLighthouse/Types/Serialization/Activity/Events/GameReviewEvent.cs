using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameReviewEvent : GameEvent
{
    [XmlElement("slot_id")]
    public ReviewSlot Slot { get; set; }

    [XmlElement("review_id")]
    public int ReviewId { get; set; }

    [XmlElement("review_modified")]
    [DefaultValue(0)]
    public long ReviewTimestamp { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        ReviewEntity review = await database.Reviews.FindAsync(this.ReviewId);
        if (review == null) return;

        SlotEntity slot = await database.Slots.FindAsync(review.SlotId);
        if (slot == null) return;

        this.Slot = ReviewSlot.CreateFromEntity(slot);
    }
}