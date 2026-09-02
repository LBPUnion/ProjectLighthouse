#nullable enable

using System.Linq;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class RecentlyPlayedCategory : SlotCategory
{
    public override string Name { get; set; } = "Recently Played";
    public override string Description { get; set; } = "Your recently played content";
    public override string IconHash { get; set; } = "g820616";
    public override string Endpoint { get; set; } = "recently_played";
    public override string Tag => "my_recently_played";
    public override string[] Sorts { get; } = ["relevance",];

    public override IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder)
    {
        return (
            from recentlyPlayed in database.RecentlyPlayed
            join slot in database.Slots.Where(queryBuilder.Build())
                on recentlyPlayed.SlotId equals slot.SlotId
            where recentlyPlayed.UserId == token.UserId
            orderby recentlyPlayed.LastPlayedAt descending
            select slot
        ).Take(CategoryConfiguration.Instance.RecentlyPlayed.MaxLevels);
    }
}
