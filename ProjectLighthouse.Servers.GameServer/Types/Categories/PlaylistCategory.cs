using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public abstract class PlaylistCategory : Category
{
    public override string[] Types { get; } = { "playlist", };

    public abstract IQueryable<PlaylistEntity> GetItems(DatabaseContext database, GameTokenEntity token);

    public override async Task<GameCategory> Serialize(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder, int numResults = 1)
    {
        List<ILbpSerializable> playlists =
            (await this.GetItems(database, token).Take(numResults).ToListAsync())
            .ToSerializableList<PlaylistEntity, ILbpSerializable>(GamePlaylist.CreateFromEntity);

        int totalPlaylists = await this.GetItems(database, token).CountAsync();
        return GameCategory.CreateFromEntity(this, new GenericSerializableList(playlists, totalPlaylists, numResults + 1));
    }
}