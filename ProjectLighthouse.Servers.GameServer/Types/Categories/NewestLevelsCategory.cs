#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class NewestLevelsCategory : Category
{
    public override string Name { get; set; } = "Newest Levels";
    public override string Description { get; set; } = "The most recently published content";
    public override string IconHash { get; set; } = "g820623";
    public override string Endpoint { get; set; } = "newest";

    public override IQueryable<SlotEntity> GetSlots(DatabaseContext database, SlotQueryBuilder queryBuilder) =>
        database.Slots.Where(queryBuilder.Build())
            .ApplyOrdering(new SlotSortBuilder<SlotEntity>().AddSort(new FirstUploadedSort()));
}