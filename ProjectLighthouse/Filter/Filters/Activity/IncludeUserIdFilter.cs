using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Activity;

public class IncludeUserIdFilter : IActivityFilter
{
    private readonly IEnumerable<int> userIds;
    private readonly EventTypeFilter eventFilter;

    public IncludeUserIdFilter(IEnumerable<int> userIds, EventTypeFilter eventFilter = null)
    {
        this.userIds = userIds;
        this.eventFilter = eventFilter;
    }

    public Expression<Func<ActivityDto, bool>> GetPredicate()
    {
        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.False<ActivityDto>();
        predicate = this.userIds.Aggregate(predicate, (current, friendId) => current.Or(a => a.Activity.UserId == friendId));
        if (this.eventFilter != null) predicate = predicate.And(this.eventFilter.GetPredicate());
        return predicate;
    }
}