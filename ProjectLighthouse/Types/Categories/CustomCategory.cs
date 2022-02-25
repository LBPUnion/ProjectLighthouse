#nullable enable
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories;

public class CustomCategory : Category
{

    public List<int> SlotIds;
    public CustomCategory(string name, string description, string endpoint, string icon, IEnumerable<int> slotIds)
    {
        this.Name = name;
        this.Description = description;
        this.IconHash = icon;
        this.Endpoint = endpoint;

        this.SlotIds = slotIds.ToList();
    }

    public CustomCategory(DatabaseCategory category)
    {
        this.Name = category.Name;
        this.Description = category.Description;
        this.IconHash = category.IconHash;
        this.Endpoint = category.Endpoint;

        this.SlotIds = category.SlotIds.ToList();
    }

    public sealed override string Name { get; set; }
    public sealed override string Description { get; set; }
    public sealed override string IconHash { get; set; }
    public sealed override string Endpoint { get; set; }
    public override Slot? GetPreviewSlot(Database database) => database.Slots.FirstOrDefault(s => s.SlotId == this.SlotIds[0]);
    public override IEnumerable<Slot> GetSlots
        (Database database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3).Where(s => this.SlotIds.Contains(s.SlotId));
    public override int GetTotalSlots(Database database) => this.SlotIds.Count;
}