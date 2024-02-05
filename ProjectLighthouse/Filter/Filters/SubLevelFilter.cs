using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class SubLevelFilter : ISlotFilter
{
    private readonly int userId;

    public SubLevelFilter(int userId)
    {
        this.userId = userId;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate() => s => !s.SubLevel || s.CreatorId == this.userId;
}