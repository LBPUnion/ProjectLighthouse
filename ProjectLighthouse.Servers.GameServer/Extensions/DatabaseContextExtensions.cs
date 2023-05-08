using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;

public static class DatabaseContextExtensions
{
    public static async Task<List<SlotBase>> GetSlots
    (
        this IQueryable<SlotEntity> queryable,
        GameTokenEntity token,
        SlotQueryBuilder queryBuilder,
        PaginationData pageData,
        ISortBuilder<SlotEntity> sortBuilder
    ) =>
        (await queryable.Where(queryBuilder.Build())
            .ApplyOrdering(sortBuilder)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(s => SlotBase.CreateFromEntity(s, token));

    public static async Task<List<SlotBase>> GetSlots
    (
        this DatabaseContext database,
        GameTokenEntity token,
        SlotQueryBuilder queryBuilder,
        PaginationData pageData,
        ISortBuilder<SlotEntity> sortBuilder
    ) =>
        (await database.Slots.Where(queryBuilder.Build())
            .ApplyOrdering(sortBuilder)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(s => SlotBase.CreateFromEntity(s, token));

    public static async Task<List<SlotBase>> GetSlots
    (
        this DatabaseContext database,
        GameTokenEntity token,
        SlotQueryBuilder queryBuilder,
        PaginationData pageData,
        ISortBuilder<SlotMetadata> sortBuilder,
        Expression<Func<SlotEntity, SlotMetadata>> selectorFunction
    ) =>
        (await database.Slots.Where(queryBuilder.Build())
            .AsQueryable()
            .Select(selectorFunction)
            .ApplyOrdering(sortBuilder)
            .Select(s => s.Slot)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(s => SlotBase.CreateFromEntity(s, token));
}