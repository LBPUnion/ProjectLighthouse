using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

public class GameSlotStreamGroup : GameStreamGroup, INeedsPreparationForSerialization
{
    [XmlElement("slot_id")]
    public ReviewSlot Slot { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        SlotEntity slot = await database.Slots.FindAsync(this.Slot.SlotId);
        if (slot == null) return;

        this.Slot = ReviewSlot.CreateFromEntity(slot);
    }
}