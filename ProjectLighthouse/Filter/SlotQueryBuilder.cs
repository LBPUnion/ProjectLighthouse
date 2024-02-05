using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter;

public class SlotQueryBuilder : IQueryBuilder<SlotEntity>
{
    private readonly List<ISlotFilter> filters;

    public SlotQueryBuilder()
    {
        this.filters = new List<ISlotFilter>();
    }

    public Expression<Func<SlotEntity, bool>> Build()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        predicate = this.filters.Aggregate(predicate, (current, filter) => current.And(filter.GetPredicate()));
        return predicate;
    }

    public SlotQueryBuilder RemoveFilter(Type type)
    {
        this.filters.RemoveAll(f => f.GetType() == type);
        return this;
    }

    #nullable enable
    public IEnumerable<ISlotFilter> GetFilters(Type type) => this.filters.Where(f => f.GetType() == type).ToList();
    #nullable disable

    public SlotQueryBuilder AddFilter(int index, ISlotFilter filter)
    {
        this.filters.Insert(index, filter);
        return this;
    }

    public SlotQueryBuilder Clone()
    {
        SlotQueryBuilder clone = new();
        clone.filters.AddRange(this.filters);
        return clone;
    }

    public SlotQueryBuilder AddFilter(ISlotFilter filter)
    {
        this.filters.Add(filter);
        return this;
    }
}