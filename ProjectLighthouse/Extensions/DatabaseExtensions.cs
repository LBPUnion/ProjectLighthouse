using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class DatabaseExtensions
{
    public static IQueryable<SlotEntity> ByGameVersion
        (this DbSet<SlotEntity> set, GameVersion gameVersion, bool includeSublevels = false, bool includeCreator = false)
        => set.AsQueryable().ByGameVersion(gameVersion, includeSublevels, includeCreator);

    public static IQueryable<SlotEntity> ByGameVersion
        (this IQueryable<SlotEntity> query, GameVersion gameVersion, bool includeSublevels = false, bool includeCreator = false, bool includeDeveloperLevels = false)
    {
        query = query.Where(s => s.Type == SlotType.User || (s.Type == SlotType.Developer && includeDeveloperLevels));

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

    public static IQueryable<ReviewEntity> ByGameVersion(this IQueryable<ReviewEntity> queryable, GameVersion gameVersion, bool includeSublevels = false)
    {
        IQueryable<ReviewEntity> query = queryable;

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

    public static async Task RemoveWhere<T>(this DbSet<T> queryable, Expression<Func<T, bool>> predicate) where T : class 
        => await queryable.Where(predicate).ExecuteDeleteAsync();
}