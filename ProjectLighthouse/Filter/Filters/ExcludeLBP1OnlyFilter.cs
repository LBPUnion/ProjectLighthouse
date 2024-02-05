using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Filter.Filters;

public class ExcludeLBP1OnlyFilter : ISlotFilter
{
    private readonly int userId;
    private readonly GameVersion targetGameVersion;

    public ExcludeLBP1OnlyFilter(int userId, GameVersion targetGameVersion)
    {
        this.userId = userId;
        this.targetGameVersion = targetGameVersion;
    }

    public Expression<Func<SlotEntity, bool>> GetPredicate() =>
        s => !s.Lbp1Only || s.CreatorId == this.userId || this.targetGameVersion == GameVersion.LittleBigPlanet1;
}