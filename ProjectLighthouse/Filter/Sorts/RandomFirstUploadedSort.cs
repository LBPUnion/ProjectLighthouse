using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Filter.Sorts;

public class RandomFirstUploadedSort : ISlotSort
{
    private const double biasFactor = .8f;

    public Expression<Func<SlotEntity, dynamic>> GetExpression() =>
        s => EF.Functions.Random() * (s.FirstUploaded * biasFactor);
}