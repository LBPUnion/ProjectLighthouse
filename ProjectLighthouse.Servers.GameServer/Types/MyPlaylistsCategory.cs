using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types;

public class MyPlaylistsCategory : PlaylistCategory
{
    public override string Name { get; set; } = "My Playlists";
    public override string Description { get; set; } = "Your playlists";
    public override string IconHash { get; set; } = "g820613";
    public override string Endpoint { get; set; } = "my_playlists";
    public override string Tag => "my_playlists";
    public override string[] Types { get; } = { "playlist", };

    public override IQueryable<PlaylistEntity> GetItems(DatabaseContext database, GameTokenEntity token) =>
        database.Playlists.Where(p => p.CreatorId == token.UserId).OrderByDescending(p => p.PlaylistId);
}