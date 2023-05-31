using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Types.Misc;

namespace LBPUnion.ProjectLighthouse.Filter.Sorts.Metadata;

public class ThumbsUpSort : ISort<SlotMetadata>
{
    public Expression<Func<SlotMetadata, dynamic>> GetExpression() => s => s.ThumbsUp;
}