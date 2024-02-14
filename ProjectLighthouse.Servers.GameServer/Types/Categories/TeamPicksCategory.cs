#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class TeamPicksCategory : SlotCategory
{
    public override string Name { get; set; } = "Team Picks";
    public override string Description { get; set; } = "Community Team Picks";
    public override string IconHash { get; set; } = "g820626";
    public override string Endpoint { get; set; } = "team_picks";
    public override string Tag => "team_picks";

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder) =>
        database.Slots.Where(queryBuilder.Clone().AddFilter(new TeamPickFilter()).Build())
            .ApplyOrdering(new SlotSortBuilder<SlotEntity>().AddSort(new TeamPickSort()).AddSort(new FirstUploadedSort()));
}