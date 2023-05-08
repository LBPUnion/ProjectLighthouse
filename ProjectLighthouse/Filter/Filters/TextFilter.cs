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
        string[] keywords = this.filter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        foreach (string keyword in keywords)
        {
            predicate = predicate.Or(s =>
                s.Name.Contains(keyword) ||
                s.Description.ToLower().Contains(keyword) ||
                s.SlotId.ToString().Equals(keyword));
            predicate = predicate.Or(s => s.Creator != null && s.Creator.Username.Contains(keyword));
        }
        return predicate;
    }
}