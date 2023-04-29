using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("slot")]
public class GameDeveloperSlot : SlotBase, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int SlotId { get; set; }

    [XmlElement("id")]
    public int InternalSlotId { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; } = "developer";

    [XmlElement("playerCount")]
    public int PlayerCount { get; set; }

    [XmlElement("commentCount")]
    public int CommentCount { get; set; }

    [XmlElement("photoCount")]
    public int PhotoCount { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        var stats = await database.Slots.Select(_ => new
        {
            CommentCount = database.Comments.Count(c => c.TargetId == this.SlotId && c.Type == CommentType.Level),
            PhotoCount = database.Photos.Count(p => p.SlotId == this.SlotId),
        }).FirstAsync();
        ReflectionHelper.CopyAllFields(stats, this);
        this.PlayerCount = RoomHelper.Rooms
            .Where(r => r.Slot.SlotType == SlotType.Developer && r.Slot.SlotId == this.InternalSlotId)
            .Sum(r => r.PlayerIds.Count);
    }
}