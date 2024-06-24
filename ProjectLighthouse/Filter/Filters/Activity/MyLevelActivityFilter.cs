using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Activity;

public class MyLevelActivityFilter : IActivityFilter
{
    private readonly int userId;
    private readonly EventTypeFilter eventFilter;

    public MyLevelActivityFilter(int userId, EventTypeFilter eventFilter = null)
    {
        this.userId = userId;
        this.eventFilter = eventFilter;
    }

    public Expression<Func<ActivityDto, bool>> GetPredicate()
    {
        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.False<ActivityDto>();
        predicate = predicate.Or(a => a.TargetSlotCreatorId == this.userId);
        if (this.eventFilter != null) predicate = predicate.And(this.eventFilter.GetPredicate());
        return predicate;
    }
}