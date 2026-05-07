using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class TextFilter : ISlotFilter
{
    private readonly string filter;

    public TextFilter(string filter)
    {
        this.filter = filter;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.False<SlotEntity>();
        string trimmed = this.filter.Trim();
        predicate = predicate.Or(s =>
            s.Name.Contains(trimmed) ||
            s.SlotId.ToString().Equals(trimmed));
        predicate = predicate.Or(s => s.Creator != null && s.Creator.Username.Contains(trimmed));
        return predicate;
    }
}