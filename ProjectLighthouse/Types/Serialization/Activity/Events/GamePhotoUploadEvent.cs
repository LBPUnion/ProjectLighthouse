using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GamePhotoUploadEvent : GameEvent
{
    [XmlElement("photo_id")]
    public int PhotoId { get; set; }

    [XmlElement("object_slot_id")]
    [DefaultValue(null)]
    public ReviewSlot Slot { get; set; }

    [XmlElement("user_in_photo")]
    public List<string> PhotoParticipants { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        PhotoEntity photo = await database.Photos.Where(p => p.PhotoId == this.PhotoId)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User)
            .FirstOrDefaultAsync();
        if (photo == null) return;

        this.PhotoParticipants = photo.PhotoSubjects.Select(ps => ps.User.Username).ToList();
        this.PhotoId = photo.PhotoId;

        if (photo.SlotId == null) return;

        SlotEntity slot = await database.Slots.FindAsync(photo.SlotId);
        if (slot == null) return;

        if (slot.Type == SlotType.User) this.Slot = ReviewSlot.CreateFromEntity(slot);
    }
}