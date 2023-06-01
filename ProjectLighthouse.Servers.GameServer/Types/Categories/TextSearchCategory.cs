using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class TextSearchCategory : SlotCategory
{
    public override string Name { get; set; } = "";
    public override string Description { get; set; } = "";
    public override string IconHash { get; set; } = "";
    public override string Endpoint { get; set; } = "text";
    public override string Tag => "text";

    public override IQueryable<SlotEntity> GetItems
        (DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder) =>
        database.Slots.Where(queryBuilder.Build())
            .ApplyOrdering(new SlotSortBuilder<SlotEntity>().AddSort(new TotalPlaysSort()));
}