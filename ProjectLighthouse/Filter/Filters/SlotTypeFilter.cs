using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class SlotTypeFilter : ISlotFilter
{
    private readonly SlotType slotType;

    public SlotTypeFilter(SlotType slotType)
    {
        this.slotType = slotType;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate() => s => s.Type == this.slotType;
}