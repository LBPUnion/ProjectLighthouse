using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class PlayerCountFilter : ISlotFilter
{
    private readonly int minPlayers;
    private readonly int maxPlayers;

    public PlayerCountFilter(int minPlayers = 1, int maxPlayers = 4)
    {
        this.minPlayers = minPlayers;
        this.maxPlayers = maxPlayers;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        predicate = predicate.And(s => s.MinimumPlayers >= this.minPlayers);
        predicate = predicate.And(s => s.MaximumPlayers <= this.maxPlayers);

        return predicate;
    }
}