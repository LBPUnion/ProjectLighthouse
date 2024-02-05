#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("photo")]
[XmlType("photo")]
public class GamePhoto : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int CreatorId { get; set; }

    [XmlElement("id")]
    public int PhotoId { get; set; }

    [XmlElement("author")]
    public string AuthorUsername { get; set; } = "";

    // Uses seconds instead of milliseconds for some reason
    [XmlAttribute("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("small")]
    public string SmallHash { get; set; } = "";

    [XmlElement("medium")]
    public string MediumHash { get; set; } = "";

    [XmlElement("large")]
    public string LargeHash { get; set; } = "";

    [XmlElement("plan")]
    public string PlanHash { get; set; } = "";

    [XmlElement("slot")]
    public PhotoSlot? LevelInfo { get; set; }

    [XmlArray("subjects")]
    [XmlArrayItem("subject")]
    public List<GamePhotoSubject>? Subjects { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        // Fetch slot data
        if (this.LevelInfo != null)
        {
            this.LevelInfo = await database.Slots.Where(s => s.SlotId == this.LevelInfo.SlotId)
                .Select(s => new
                {
                    s.InternalSlotId,
                    s.Type,
                })
                .Select(s => new PhotoSlot
                {
                    SlotId = s.Type == SlotType.User ? this.LevelInfo.SlotId : s.InternalSlotId,
                    SlotType = s.Type,
                })
                .FirstOrDefaultAsync();
        }

        // Fetch creator username
        this.AuthorUsername = await database.Users.Where(u => u.UserId == this.CreatorId)
                                  .Select(u => u.Username)
                                  .FirstOrDefaultAsync() ?? "";

        // Fetch photo subject usernames
        foreach (GamePhotoSubject photoSubject in this.Subjects ?? Enumerable.Empty<GamePhotoSubject>())
        {
            photoSubject.Username = await database.Users.Where(u => u.UserId == photoSubject.UserId)
                                        .Select(u => u.Username)
                                        .FirstOrDefaultAsync() ?? "";
        }
    }

    public static GamePhoto CreateFromEntity(PhotoEntity entity) =>
        new()
        {
            PhotoId = entity.PhotoId,
            CreatorId = entity.CreatorId,
            LevelInfo = new PhotoSlot
            {
                SlotId = entity.SlotId.GetValueOrDefault(),
            },
            // Timestamps are uploaded and stored in seconds but game expects milliseconds
            Timestamp = entity.Timestamp * 1000,
            SmallHash = entity.SmallHash,
            MediumHash = entity.MediumHash,
            LargeHash = entity.LargeHash,
            PlanHash = entity.PlanHash,
            Subjects = entity.PhotoSubjects.ToSerializableList(GamePhotoSubject.CreateFromEntity),
        };

}