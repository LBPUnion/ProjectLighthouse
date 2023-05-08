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

    public Expression<Func<SlotEntity, bool>> GetPredicate() =>
        PredicateExtensions.True<SlotEntity>()
            .And(s => s.MinimumPlayers >= this.minPlayers)
            .And(s => s.MaximumPlayers <= this.maxPlayers);
}