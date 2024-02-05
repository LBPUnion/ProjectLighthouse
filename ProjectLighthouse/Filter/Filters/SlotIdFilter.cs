using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class SlotIdFilter : ISlotFilter
{
    private readonly List<int> slotIds;

    public SlotIdFilter(List<int> slotIds)
    {
        this.slotIds = slotIds;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.False<SlotEntity>();
        predicate = this.slotIds.Aggregate(predicate, (current, slotId) => current.Or(s => s.SlotId == slotId));
        return predicate;
    }
}