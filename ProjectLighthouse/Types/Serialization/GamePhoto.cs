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

    [XmlElement("slot")]
    public PhotoSlot? LevelInfo;

    [XmlArray("subjects")]
    [XmlArrayItem("subject")]
    public List<GamePhotoSubject>? Subjects;

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

    public async Task PrepareSerialization(DatabaseContext database)
    {
        if (this.LevelInfo?.SlotId == 0) this.LevelInfo = null;

        // Fetch slot data
        if (this.LevelInfo != null)
        {
            var partialSlot = await database.Slots.Where(s => s.SlotId == this.LevelInfo.SlotId)
                .Select(s => new
                {
                    s.InternalSlotId,
                    s.Type,
                })
                .FirstOrDefaultAsync();

            if (partialSlot != null)
            {
                this.LevelInfo.SlotType = partialSlot.Type;

                if (partialSlot.Type == SlotType.Developer)
                    this.LevelInfo.SlotId = partialSlot.InternalSlotId;
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