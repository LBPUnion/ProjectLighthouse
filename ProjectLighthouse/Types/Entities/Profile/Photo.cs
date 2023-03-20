#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

[XmlRoot("slot")]
public class PhotoSlot
{
    [XmlAttribute("type")]
    public SlotType SlotType { get; set; }

    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlElement("rootLevel")]
    public string? RootLevel { get; set; }

    [XmlElement("name")]
    public string? LevelName { get; set; }
}

[XmlRoot("photo")]
[XmlType("photo")]
public class Photo
{

    [NotMapped]
    [XmlElement("slot")]
    public PhotoSlot? XmlLevelInfo;

    [NotMapped]
    [XmlArray("subjects")]
    [XmlArrayItem("subject")]
    public List<PhotoSubject>? XmlSubjects;

    [Key]
    public int PhotoId { get; set; }

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

    [XmlIgnore]
    public virtual ICollection<PhotoSubject> PhotoSubjects { get; set; } = new HashSet<PhotoSubject>();

    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public User? Creator { get; set; }

    public int? SlotId { get; set; }

    [ForeignKey(nameof(SlotId))]
    public Slot? Slot { get; set; }

    public string Serialize()
    {
        using DatabaseContext database = new();
        var partialSlot = database.Slots.Where(s => s.SlotId == this.SlotId.GetValueOrDefault())
            .Select(s => new
            {
                s.InternalSlotId,
                s.Type,
            })
            .FirstOrDefault();
        if (partialSlot == null) return this.Serialize(0, SlotType.User);

        int serializedSlotId = partialSlot.InternalSlotId;
        if (serializedSlotId == 0) serializedSlotId = this.SlotId.GetValueOrDefault();

        return this.Serialize(serializedSlotId, partialSlot.Type);
    }

    public string Serialize(int slotId, SlotType slotType)
    {
        
        string slot = LbpSerializer.TaggedStringElement("slot", LbpSerializer.StringElement("id", slotId), "type", slotType.ToString().ToLower());
        if (slotId == 0) slot = "";

        string subjectsAggregate = this.PhotoSubjects.Aggregate(string.Empty, (s, subject) => s + subject.Serialize());

        string photo = LbpSerializer.StringElement("id", this.PhotoId) +
                       LbpSerializer.StringElement("small", this.SmallHash) +
                       LbpSerializer.StringElement("medium", this.MediumHash) +
                       LbpSerializer.StringElement("large", this.LargeHash) +
                       LbpSerializer.StringElement("plan", this.PlanHash) +
                       LbpSerializer.StringElement("author", this.Creator?.Username) +
                       LbpSerializer.StringElement("subjects", subjectsAggregate) +
                       slot;

        return LbpSerializer.TaggedStringElement("photo", photo, "timestamp", this.Timestamp * 1000);
    }
}