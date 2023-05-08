using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;

namespace LBPUnion.ProjectLighthouse.Filter.Sorts;

public class LastUpdatedSort : ISlotSort
{
    public Expression<Func<SlotEntity, dynamic>> GetExpression() => s => s.LastUpdated;
}