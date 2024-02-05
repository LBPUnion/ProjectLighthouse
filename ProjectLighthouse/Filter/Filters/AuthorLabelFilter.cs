using System;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class AuthorLabelFilter : ISlotFilter
{
    private readonly string[] labels;

    public AuthorLabelFilter(params string[] labels)
    {
        this.labels = labels;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        predicate = this.labels.Aggregate(predicate,
            (current, label) => current.And(s => s.AuthorLabels.Contains(label)));
        return predicate;
    }
}