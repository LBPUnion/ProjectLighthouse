#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class CustomCategory : SlotCategory
{
    private readonly List<int> slotIds;
    public CustomCategory(string name, string description, string endpoint, string icon, IEnumerable<int> slotIds)
    {
        this.Name = name;
        this.Description = description;
        this.IconHash = icon;
        this.Endpoint = endpoint;

        this.slotIds = slotIds.ToList();
    }

    public CustomCategory(DatabaseCategoryEntity category)
    {
        this.Name = category.Name;
        this.Description = category.Description;
        this.IconHash = category.IconHash;
        this.Endpoint = category.Endpoint;

        this.slotIds = category.SlotIds.ToList();
    }

    public sealed override string Name { get; set; }
    public sealed override string Description { get; set; }
    public sealed override string IconHash { get; set; }
    public sealed override string Endpoint { get; set; }

    public override string Tag => "custom_category";

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity entity, SlotQueryBuilder queryBuilder)
    {
        queryBuilder.Clone().AddFilter(new SlotIdFilter(this.slotIds));
        return database.Slots.Where(queryBuilder.Build());
    }
}