using System;
using System.Linq.Expressions;

namespace LBPUnion.ProjectLighthouse.Types.Filter;

public interface IQueryBuilder<T>
{
    public Expression<Func<T, bool>> Build();
}