#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class MostPlayedCategory : Category
{
    public override string Name { get; set; } = "Most Played";
    public override string Description { get; set; } = "The most played content";
    public override string IconHash { get; set; } = "g820608";
    public override string Endpoint { get; set; } = "mostUniquePlays";

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, SlotQueryBuilder queryBuilder) =>
        database.Slots.Where(queryBuilder.Build())
            .OrderByDescending(s => s.PlaysLBP1Unique + s.PlaysLBP2Unique + s.PlaysLBP3Unique)
            .ThenByDescending(s => s.PlaysLBP1 + s.PlaysLBP2 + s.PlaysLBP3);

}