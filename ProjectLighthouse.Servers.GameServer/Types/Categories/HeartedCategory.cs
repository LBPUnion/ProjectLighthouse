#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class HeartedCategory : SlotCategory
{
    public override string Name { get; set; } = "My Hearted Content";
    public override string Description { get; set; } = "Content you've hearted";
    public override string IconHash { get; set; } = "g820611";
    public override string Endpoint { get; set; } = "hearted_levels";
    public override string Tag => "my_hearted_levels";

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder) =>
        database.HeartedLevels.Where(h => h.UserId == token.UserId)
            .OrderByDescending(h => h.HeartedLevelId)
            .Select(h => h.Slot)
            .Where(queryBuilder.Build());
}