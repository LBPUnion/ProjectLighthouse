using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class DatabaseExtensions
{
    public static async Task<bool> Has<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> predicate) 
        => await queryable.FirstOrDefaultAsync(predicate) != null;

    public static async Task RemoveWhere<T>(this DbSet<T> queryable, Expression<Func<T, bool>> predicate) where T : class 
        => await queryable.Where(predicate).ExecuteDeleteAsync();
}