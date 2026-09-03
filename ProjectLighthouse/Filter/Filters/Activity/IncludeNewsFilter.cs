using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Activity;

public class IncludeNewsFilter : IActivityFilter
{
    public Expression<Func<ActivityDto, bool>> GetPredicate() =>
        a => (a.Activity is NewsActivityEntity && a.Activity.Type == EventType.NewsPost) ||
             a.Activity.Type == EventType.MMPickLevel;
}