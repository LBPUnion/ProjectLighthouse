using System.Linq;

namespace LBPUnion.ProjectLighthouse.Types.Filter.Sorts;

public interface ISortBuilder<T>
{
    public IOrderedQueryable<T> Build(IQueryable<T> queryable);
}