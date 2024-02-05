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

    /// <summary>
    ///   Deletes all records matching a given predicate
    ///   <para>Deletes are executed immediately without calling SaveChanges()</para>
    /// </summary>
    /// <param name="dbSet">The database set to source from</param>
    /// <param name="predicate">The predicate used to determine which records to delete</param>
    /// <typeparam name="T">The record type contained within the DbSet</typeparam>
    public static async Task RemoveWhere<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate) where T : class 
        => await dbSet.Where(predicate).ExecuteDeleteAsync();
}