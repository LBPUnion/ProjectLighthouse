#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class QueueCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Queue";
    public override string Description { get; set; } = "Your queued content";
    public override string IconHash { get; set; } = "g820614";
    public override string Endpoint { get; set; } = "queue";

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, UserEntity user, SlotQueryBuilder queryBuilder)
    {
        return database.QueuedLevels.Where(q => q.UserId == user.UserId)
            .OrderByDescending(q => q.QueuedLevelId)
            .Select(q => q.Slot)
            .Where(queryBuilder.Build());
    }
}