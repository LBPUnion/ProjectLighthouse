using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;

namespace LBPUnion.ProjectLighthouse.Filter.Sorts;

public class TotalPlaysSort : ISlotSort
{
    public Expression<Func<SlotEntity, dynamic>> GetExpression() => s => s.PlaysLBP1 + s.PlaysLBP2 + s.PlaysLBP3;
}