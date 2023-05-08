using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class ExcludeLBP1OnlyFilter : ISlotFilter
{
    private readonly int userId;

    public ExcludeLBP1OnlyFilter(int userId)
    {
        this.userId = userId;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        predicate = predicate.And(s => !s.Lbp1Only || s.CreatorId == this.userId);
        return predicate;
    }
}