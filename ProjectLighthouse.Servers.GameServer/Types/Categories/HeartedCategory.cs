#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class HeartedCategory : CategoryWithUser
{
    public override string Name { get; set; } = "My Hearted Content";
    public override string Description { get; set; } = "Content you've hearted";
    public override string IconHash { get; set; } = "g820611";
    public override string Endpoint { get; set; } = "hearted";

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, UserEntity user, SlotQueryBuilder queryBuilder)
    {
        return database.HeartedLevels.Where(h => h.UserId == user.UserId)
            .OrderByDescending(h => h.HeartedLevelId)
            .Select(h => h.Slot)
            .Where(queryBuilder.Build());
    }
}