using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Filter.Sorts;

public class RandomSort : ISlotSort
{
    public Expression<Func<SlotEntity, dynamic>> GetExpression() => _ => EF.Functions.Random();
}