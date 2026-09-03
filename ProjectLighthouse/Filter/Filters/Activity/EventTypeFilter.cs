#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Activity;

public class EventTypeFilter : IActivityFilter
{
    private readonly EventType[] events;

    public EventTypeFilter(params EventType[] events)
    {
        this.events = events;
    }

    public Expression<Func<ActivityDto, bool>> GetPredicate()
    {
        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.False<ActivityDto>();
        predicate = this.events.Aggregate(predicate,
            (current, eventType) => current.Or(a => a.Activity.Type == eventType));
        return predicate;
    } 
}