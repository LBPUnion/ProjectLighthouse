using System.Linq;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Reviews;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

public static class DatabaseExtensions
{
    public static IQueryable<Slot> ByGameVersion
        (this DbSet<Slot> set, GameVersion gameVersion, bool includeSublevels = false)
        => set.AsQueryable().ByGameVersion(gameVersion, includeSublevels);

    public static IQueryable<Slot> ByGameVersion(this IQueryable<Slot> queryable, GameVersion gameVersion, bool includeSublevels = false)
    {
        IQueryable<Slot> query = queryable.Include(s => s.Creator).Include(s => s.Location);

        if (gameVersion == GameVersion.LittleBigPlanetVita || gameVersion == GameVersion.LittleBigPlanetPSP || gameVersion == GameVersion.Unknown)
        {
            query = query.Where(s => s.GameVersion == gameVersion);
        }
        else
        {
            query = query.Where(s => s.GameVersion <= gameVersion);
        }

        if (!includeSublevels) query = query.Where(s => !s.SubLevel);

        return query;
    }

    public static IQueryable<Review> ByGameVersion(this IQueryable<Review> queryable, GameVersion gameVersion, bool includeSublevels = false)
    {
        IQueryable<Review> query = queryable.Include(r => r.Slot).Include(r => r.Slot.Creator).Include(r => r.Slot.Location);

        if (gameVersion == GameVersion.LittleBigPlanetVita || gameVersion == GameVersion.LittleBigPlanetPSP || gameVersion == GameVersion.Unknown)
        {
            query = query.Where(r => r.Slot.GameVersion == gameVersion);
        }
        else
        {
            query = query.Where(r => r.Slot.GameVersion <= gameVersion);
        }

        if (!includeSublevels) query = query.Where(r => !r.Slot.SubLevel);

        return query;
    }
}