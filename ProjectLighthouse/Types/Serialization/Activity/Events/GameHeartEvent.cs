using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameHeartUserEvent : GameEvent
{
    [XmlIgnore]
    public int TargetUserId { get; set; }

    [XmlElement("object_user")]
    public string TargetUsername { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        UserEntity targetUser = await database.Users.FindAsync(this.TargetUserId);
        if (targetUser == null) return;

        this.TargetUsername = targetUser.Username;
    }
}

public class GameHeartLevelEvent : GameEvent
{
    [XmlElement("object_slot_id")]
    public ReviewSlot TargetSlot { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        SlotEntity slot = await database.Slots.FindAsync(this.TargetSlot.SlotId);
        if (slot == null) return;

        this.TargetSlot = ReviewSlot.CreateFromEntity(slot);
    }
}