using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter;

public class ActivityQueryBuilder : IQueryBuilder<ActivityDto>
{
    private readonly List<IActivityFilter> filters;

    public ActivityQueryBuilder()
    {
        this.filters = new List<IActivityFilter>();
    }

    public Expression<Func<ActivityDto, bool>> Build()
    {
        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.True<ActivityDto>();
        predicate = this.filters.Aggregate(predicate, (current, filter) => current.And(filter.GetPredicate()));
        return predicate;
    }

    public ActivityQueryBuilder RemoveFilter(Type type)
    {
        this.filters.RemoveAll(f => f.GetType() == type);
        return this;
    }

    #nullable enable
    public IEnumerable<IActivityFilter> GetFilters(Type type) => this.filters.Where(f => f.GetType() == type).ToList();
    #nullable disable

    public ActivityQueryBuilder AddFilter(int index, IActivityFilter filter)
    {
        this.filters.Insert(index, filter);
        return this;
    }

    public ActivityQueryBuilder Clone()
    {
        ActivityQueryBuilder clone = new();
        clone.filters.AddRange(this.filters);
        return clone;
    }

    public ActivityQueryBuilder AddFilter(IActivityFilter filter)
    {
        this.filters.Add(filter);
        return this;
    }
}