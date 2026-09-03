using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Slot;

public class ExcludeMovePackFilter : ISlotFilter
{
    public Expression<Func<SlotEntity, bool>> GetPredicate() => s => !s.MoveRequired;
}