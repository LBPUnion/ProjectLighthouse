using System;
using System.Linq.Expressions;

namespace LBPUnion.ProjectLighthouse.Types.Filter.Sorts;

public interface ISort<T>
{
    public Expression<Func<T, dynamic>> GetExpression();
}