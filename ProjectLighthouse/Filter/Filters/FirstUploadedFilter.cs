using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class FirstUploadedFilter : ISlotFilter
{
    private readonly long start;
    private readonly long end;

    public FirstUploadedFilter(long start, long end = long.MaxValue)
    {
        this.start = start;
        this.end = end;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        predicate = predicate.And(s => s.FirstUploaded > this.start);

        // Exclude to optimize query
        if (this.end != long.MaxValue) predicate = predicate.And(s => s.FirstUploaded < this.end);

        return predicate;
    }
}