using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;

namespace LBPUnion.ProjectLighthouse.Filter;

public class SlotSortBuilder<T> : ISortBuilder<T>
{
    private readonly List<ISort<T>> sorts;
    private bool sortDescending;

    public SlotSortBuilder()
    {
        this.sorts = new List<ISort<T>>();
        this.sortDescending = true;
    }

    public SlotSortBuilder<T> AddSort(ISort<T> sort)
    {
        this.sorts.Add(sort);
        return this;
    }

    public SlotSortBuilder<T> SortDescending(bool descending)
    {
        this.sortDescending = descending;
        return this;
    }

    public IOrderedQueryable<T> Build(IQueryable<T> queryable)
    {
        IOrderedQueryable<T> orderedQueryable = (IOrderedQueryable<T>)queryable;
        // Probably not the best way to do this but to convert from IQueryable to IOrderedQueryable you have to 
        // OrderBy some field before you can call ThenBy. One way to do this is call OrderBy(s => 0) but this
        // generates some extra SQL so I've settled on this
        bool usedFirstOrder = false;
        foreach (ISort<T> sort in this.sorts)
        {
            if (this.sortDescending)
            {
                orderedQueryable = !usedFirstOrder
                    ? orderedQueryable.OrderByDescending(sort.GetExpression())
                    : orderedQueryable.ThenByDescending(sort.GetExpression());
            }
            else
            {
                orderedQueryable = !usedFirstOrder
                    ? orderedQueryable.OrderBy(sort.GetExpression())
                    : orderedQueryable.ThenBy(sort.GetExpression());
            }

            usedFirstOrder = true;
        }
        return orderedQueryable;
    }
}