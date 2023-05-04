#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

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

    public CustomCategory(DatabaseCategoryEntity category)
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
    public override SlotEntity? GetPreviewSlot(DatabaseContext database) => database.Slots.FirstOrDefault(s => s.SlotId == this.SlotIds[0] && !s.CrossControllerRequired);
    public override IQueryable<SlotEntity> GetSlots
        (DatabaseContext database, int pageStart, int pageSize)
        => database.Slots.ByGameVersion(GameVersion.LittleBigPlanet3).Where(s => this.SlotIds.Contains(s.SlotId) && !s.CrossControllerRequired);
    public override int GetTotalSlots(DatabaseContext database) => this.SlotIds.Count;
}