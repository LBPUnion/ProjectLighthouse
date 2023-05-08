using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class SubLevelFilter : ISlotFilter
{
    private readonly bool includeSubLevels; 

    public SubLevelFilter(bool includeSubLevels = false)
    {
        this.includeSubLevels = includeSubLevels;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        if (!this.includeSubLevels)
        {
            predicate = predicate.And(s => !s.SubLevel);
        }
        return predicate;
    }          
}