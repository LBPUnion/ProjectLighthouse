using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class GameVersionFilter : ISlotFilter
{
    private readonly GameVersion targetVersion;

    public GameVersionFilter(GameVersion targetVersion)
    {
        this.targetVersion = targetVersion;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate()
    {
        Expression<Func<SlotEntity, bool>> predicate = PredicateExtensions.True<SlotEntity>();
        predicate =
            this.targetVersion is GameVersion.LittleBigPlanetVita or GameVersion.LittleBigPlanetPSP
                or GameVersion.Unknown
                ? predicate.And(s => s.GameVersion == this.targetVersion)
                : predicate.And(s => s.GameVersion <= this.targetVersion);
        return predicate;
    }
}