using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class AdventureFilter : ISlotFilter
{
    public Expression<Func<SlotEntity, bool>> GetPredicate() =>
        PredicateExtensions.True<SlotEntity>().And(s => s.IsAdventurePlanet);
}