#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class MostPlayedCategory : SlotCategory
{
    public override string Name { get; set; } = "Most Played";
    public override string Description { get; set; } = "The most played content";
    public override string IconHash { get; set; } = "g820608";
    public override string Endpoint { get; set; } = "most_played";
    public override string Tag => "most_played";

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder) =>
        database.Slots.Where(queryBuilder.Build())
            .OrderByDescending(s => s.PlaysLBP1Unique + s.PlaysLBP2Unique + s.PlaysLBP3Unique)
            .ThenByDescending(s => s.PlaysLBP1 + s.PlaysLBP2 + s.PlaysLBP3);
}