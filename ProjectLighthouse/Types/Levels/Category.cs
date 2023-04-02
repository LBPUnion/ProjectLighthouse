#nullable enable
using System.Collections.Generic;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

[XmlType("category")]
[XmlRoot("category")]
public abstract class Category
{
    [XmlElement("name")]
    public abstract string Name { get; set; }

    [XmlElement("description")]
    public abstract string Description { get; set; }

    [XmlElement("icon")]
    public abstract string IconHash { get; set; }

    [XmlIgnore]
    public abstract string Endpoint { get; set; }

    [XmlElement("url")]
    public string IngameEndpoint {
        get => $"/searches/{this.Endpoint}";
        set => this.Endpoint = value.Replace("/searches/", "");
    }

    public abstract SlotEntity? GetPreviewSlot(DatabaseContext database);

    public abstract IEnumerable<SlotEntity> GetSlots(DatabaseContext database, int pageStart, int pageSize);

    public abstract int GetTotalSlots(DatabaseContext database);

    public GameCategory Serialize(DatabaseContext database)
    {
        List<SlotBase> slots = new();
        SlotEntity? previewSlot = this.GetPreviewSlot(database);
        if (previewSlot != null)
            slots.Add(SlotBase.CreateFromEntity(previewSlot, GameVersion.LittleBigPlanet3, -1));
        
        int totalSlots = this.GetTotalSlots(database);
        return GameCategory.CreateFromEntity(this, new GenericSlotResponse(slots, totalSlots, 2));
    }
}