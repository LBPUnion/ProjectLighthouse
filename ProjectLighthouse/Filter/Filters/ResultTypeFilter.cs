using System;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class ResultTypeFilter : ISlotFilter
{
    private readonly string[] results;

    public ResultTypeFilter(params string[] results)
    {
        this.results = results;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate() => this.results.Contains("slot") ? s => true : s => false;
}