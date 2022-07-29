using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Reviews;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class DatabaseExtensions
{
    public static IQueryable<Slot> ByGameVersion
        (this DbSet<Slot> set, GameVersion gameVersion, bool includeSublevels = false, bool includeCreatorAndLocation = false)
        => set.AsQueryable().ByGameVersion(gameVersion, includeSublevels, includeCreatorAndLocation);

    public static IQueryable<Slot> ByGameVersion
        (this IQueryable<Slot> query, GameVersion gameVersion, bool includeSublevels = false, bool includeCreatorAndLocation = false)
    {
        query = query.Where(s => s.Type == "user");

        if (includeCreatorAndLocation)
        {
            query = query.Include(s => s.Creator).Include(s => s.Location);
        }

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

    public static async Task<bool> Has<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate) 
        => await queryable.FirstOrDefaultAsync(predicate) != null;

    public static void RemoveWhere<T>(this DbSet<T> queryable, Expression<Func<T, bool>> predicate) where T : class 
        => queryable.RemoveRange(queryable.Where(predicate));
}