using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameRateLevelEvent : GameEvent
{
    [XmlElement("object_slot_id")]
    public ReviewSlot Slot { get; set; }

    [XmlElement("rating")]
    public double Rating { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        SlotEntity slot = await database.Slots.FindAsync(this.Slot.SlotId);
        if (slot == null) return;

        this.Slot = ReviewSlot.CreateFromEntity(slot);

        this.Rating = await database.RatedLevels.Where(r => r.SlotId == slot.SlotId && r.UserId == this.UserId)
            .Select(r => r.RatingLBP1)
            .FirstOrDefaultAsync();
    }
}