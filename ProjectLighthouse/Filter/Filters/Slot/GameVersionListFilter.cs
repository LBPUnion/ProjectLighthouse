using System;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Slot;

public class GameVersionListFilter : ISlotFilter
{
    private readonly GameVersion[] versions;

    public GameVersionListFilter(params GameVersion[] versions)
    {
        this.versions = versions;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate() =>
        this.versions.Aggregate(PredicateExtensions.False<SlotEntity>(),
            (current, version) => PredicateExtensions.Or<SlotEntity>(current, s => s.GameVersion == version));
}