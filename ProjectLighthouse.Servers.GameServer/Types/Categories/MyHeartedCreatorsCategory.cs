using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public class MyHeartedCreatorsCategory : UserCategory
{
    public override string Name { get; set; } = "My Hearted Creators";
    public override string Description { get; set; } = "Creators you've hearted";
    public override string IconHash { get; set; } = "g820612";
    public override string Endpoint { get; set; } = "favourite_creators";
    public override string Tag => "favourite_creators";

    public override IQueryable<UserEntity> GetItems(DatabaseContext database, GameTokenEntity token) =>
        database.HeartedProfiles.Where(h => h.UserId == token.UserId)
            .OrderByDescending(h => h.UserId)
            .Include(h => h.HeartedUser)
            .Select(h => h.HeartedUser);
}