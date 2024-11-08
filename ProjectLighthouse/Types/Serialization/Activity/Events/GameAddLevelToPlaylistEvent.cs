using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameAddLevelToPlaylistEvent : GameEvent
{
    [XmlElement("object_playlist_id")]
    public int TargetPlaylistId { get; set; }

    [XmlElement("object_slot_id")]
    public ReviewSlot Slot { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        SlotEntity slot = await database.Slots.FindAsync(this.Slot.SlotId);
        if (slot == null) return;

        this.Slot = ReviewSlot.CreateFromEntity(slot);
    }
}