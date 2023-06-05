using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class GameVersionFilter : ISlotFilter
{
    private readonly GameVersion targetVersion;
    private readonly bool matchExactly;

    public GameVersionFilter(GameVersion targetVersion, bool matchExactly = false)
    {
        this.targetVersion = targetVersion;
        this.matchExactly = matchExactly;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate() =>
        this.matchExactly ||
        this.targetVersion is GameVersion.LittleBigPlanetVita or GameVersion.LittleBigPlanetPSP or GameVersion.Unknown
            ? s => s.GameVersion == this.targetVersion
            : s => s.GameVersion <= this.targetVersion;
}