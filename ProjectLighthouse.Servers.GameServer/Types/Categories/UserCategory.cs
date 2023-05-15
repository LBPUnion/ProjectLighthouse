using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public abstract class UserCategory : Category
{
    public override string[] Types { get; } = { "user", };

    public abstract IQueryable<UserEntity> GetItems(DatabaseContext database, GameTokenEntity token);

    public override async Task<GameCategory> Serialize(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder, int numResults = 1)
    {
        List<ILbpSerializable> users =
            (await this.GetItems(database, token).Take(numResults).ToListAsync())
            .ToSerializableList<UserEntity, ILbpSerializable>(GameUser.CreateFromEntity);

        int totalUsers = await this.GetItems(database, token).CountAsync();
        return GameCategory.CreateFromEntity(this, new GenericSerializableList(users, totalUsers, numResults + 1));
    }
}