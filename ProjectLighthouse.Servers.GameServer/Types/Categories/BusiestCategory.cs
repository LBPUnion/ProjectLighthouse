#nullable enable
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class BusiestCategory : SlotCategory
{
    public override string Name { get; set; } = "Busiest";
    public override string Description { get; set; } = "Levels being played right now!";
    public override string IconHash { get; set; } = "g820602";
    public override string Endpoint { get; set; } = "busiest";
    public override string Tag => "busiest";
    public override string[] Sorts { get; } = ["relevance",];

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder)
    {
        Dictionary<int, int> playerCounts = RoomHelper.GetUserLevelPlayerCounts();

        if (playerCounts.Count == 0)
            return database.Slots.Where(_ => false);

        List<int> slotIds = playerCounts.Keys.ToList();

        ParameterExpression slotParameter = Expression.Parameter(typeof(SlotEntity), "slot");

        MemberExpression slotIdProperty = Expression.Property(slotParameter, nameof(SlotEntity.SlotId));

        Expression playerCountExpression = Expression.Constant(0);

        foreach (KeyValuePair<int, int> playerCount in playerCounts)
        {
            playerCountExpression = Expression.Condition(Expression.Equal(slotIdProperty, Expression.Constant(playerCount.Key)), Expression.Constant(playerCount.Value), playerCountExpression);
        }

        Expression<Func<SlotEntity, int>> ordering = Expression.Lambda<Func<SlotEntity, int>>(playerCountExpression, slotParameter);

        return database.Slots
            .Where(slot =>
                slotIds.Contains(slot.SlotId))
            .Where(queryBuilder.Build())
            .OrderByDescending(ordering)
            .ThenByDescending(slot => slot.SlotId);
    }
}
