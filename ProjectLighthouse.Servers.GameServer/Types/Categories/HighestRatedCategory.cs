#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Misc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class HighestRatedCategory : SlotCategory
{
    public override string Name { get; set; } = "Highest Rated";
    public override string Description { get; set; } = "Community Highest Rated content";
    public override string IconHash { get; set; } = "g820603";
    public override string Endpoint { get; set; } = "thumbs";
    public override string Tag => "highest_rated";

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder) =>
        database.Slots.Select(s => new SlotMetadata
            {
                Slot = s,
                ThumbsUp = database.RatedLevels.Count(r => r.SlotId == s.SlotId && r.Rating == 1),
            })
            .OrderByDescending(s => s.ThumbsUp)
            .Select(s => s.Slot)
            .Where(queryBuilder.Build());
}