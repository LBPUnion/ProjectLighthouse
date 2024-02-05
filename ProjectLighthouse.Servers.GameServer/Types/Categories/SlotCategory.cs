using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public abstract class SlotCategory : Category
{
    public override string[] Types { get; } = { "slot", "adventure", };

    public abstract IQueryable<SlotEntity> GetItems(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder);

    public override async Task<GameCategory> Serialize(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder, int numResults = 1)
    {
        List<ILbpSerializable> slots =
            (await this.GetItems(database, token, queryBuilder).Take(numResults).ToListAsync())
            .ToSerializableList<SlotEntity, ILbpSerializable>(s => SlotBase.CreateFromEntity(s, token));

        int totalSlots = await this.GetItems(database, token, queryBuilder).CountAsync();
        return GameCategory.CreateFromEntity(this, new GenericSerializableList(slots, totalSlots, numResults+1));
    }
}