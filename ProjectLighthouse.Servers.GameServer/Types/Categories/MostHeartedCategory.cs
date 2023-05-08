#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Sorts.Metadata;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class MostHeartedCategory : Category
{
    public override string Name { get; set; } = "Most Hearted";
    public override string Description { get; set; } = "The Most Hearted Content";
    public override string IconHash { get; set; } = "g820607";
    public override string Endpoint { get; set; } = "mostHearted";

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, SlotQueryBuilder queryBuilder) =>
        database.Slots.Select(s => new SlotMetadata
            {
                Slot = s,
                Hearts = database.HeartedLevels.Count(r => r.SlotId == s.SlotId),
            })
            .ApplyOrdering(new SlotSortBuilder<SlotMetadata>().AddSort(new HeartsSort()))
            .Select(s => s.Slot)
            .Where(queryBuilder.Build());
}