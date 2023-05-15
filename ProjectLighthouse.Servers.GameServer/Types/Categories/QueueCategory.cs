#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class QueueCategory : SlotCategory
{
    public override string Name { get; set; } = "My Queue";
    public override string Description { get; set; } = "Your queued content";
    public override string IconHash { get; set; } = "g820614";
    public override string Endpoint { get; set; } = "queue";
    public override string Tag => "my_queue";

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder) =>
        database.QueuedLevels.Where(q => q.UserId == token.UserId)
            .OrderByDescending(q => q.QueuedLevelId)
            .Select(q => q.Slot)
            .Where(queryBuilder.Build());
}