using System;
using System.Linq.Expressions;

namespace LBPUnion.ProjectLighthouse.Types.Filter;

public interface IFilter<T>
{
    public Expression<Func<T, bool>> GetPredicate();
}