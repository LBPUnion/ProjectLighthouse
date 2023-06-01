#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class PlaylistController : ControllerBase
{
    private readonly DatabaseContext database;

    public PlaylistController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("playlists/{playlistId:int}/slots")]
    public async Task<IActionResult> GetPlaylistSlots(int playlistId)
    {
        PlaylistEntity? targetPlaylist = await this.database.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
        if (targetPlaylist == null) return this.BadRequest();

        GameTokenEntity token = this.GetToken();

        List<SlotBase> slots = (await this.database.Slots.Where(s => targetPlaylist.SlotIds.Contains(s.SlotId)).ToListAsync())
            .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));

        int total = targetPlaylist.SlotIds.Length;

        return this.Ok(new GenericSlotResponse(slots, total, 0));
    }

    [HttpPost("playlists/{playlistId:int}/delete")]
    public async Task<IActionResult> DeletePlaylist(int playlistId)
    {
        GameTokenEntity token = this.GetToken();

        PlaylistEntity? targetPlaylist = await this.database.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
        if (targetPlaylist == null) return this.BadRequest();

        if (token.UserId != targetPlaylist.CreatorId) return this.Unauthorized();

        this.database.Playlists.Remove(targetPlaylist);
        await this.database.SaveChangesAsync();

        return this.Ok(await this.GetUserPlaylists(token.UserId));
    }

    [HttpPost("playlists/{playlistId:int}")]
    [HttpPost("playlists/{playlistId:int}/slots")]
    [HttpPost("playlists/{playlistId:int}/slots/{slotId:int}/delete")]
    [HttpPost("playlists/{playlistId:int}/order_slots")]
    public async Task<IActionResult> UpdatePlaylist(int playlistId, int slotId)
    {
        GameTokenEntity token = this.GetToken();

        PlaylistEntity? targetPlaylist = await this.database.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
        if (targetPlaylist == null) return this.BadRequest();

        if (token.UserId != targetPlaylist.CreatorId) return this.BadRequest();

        // Delete a slot from playlist
        if (slotId != 0)
        {
            targetPlaylist.SlotIds = targetPlaylist.SlotIds.Where(s => s != slotId).ToArray();
            await this.database.SaveChangesAsync();
            return this.Ok(await this.GetUserPlaylists(token.UserId));
        }

        GamePlaylist? newPlaylist = await this.DeserializeBody<GamePlaylist>("playlist", "levels");

        if (newPlaylist == null) return this.BadRequest();

        if (newPlaylist.LevelIds != null)
        {
            HashSet<int> slotIds = new(targetPlaylist.SlotIds);

            // Reorder
            if (slotIds.SetEquals(newPlaylist.LevelIds))
            {
                targetPlaylist.SlotIds = newPlaylist.LevelIds;
            }
            // Add a level
            else
            {
                foreach (int id in newPlaylist.LevelIds)
                {
                    targetPlaylist.SlotIds = targetPlaylist.SlotIds.Append(id).ToArray();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(newPlaylist.Name)) targetPlaylist.Name = newPlaylist.Name;

        if (!string.IsNullOrWhiteSpace(newPlaylist.Description)) targetPlaylist.Description = newPlaylist.Description;

        await this.database.SaveChangesAsync();

        return this.Ok(GamePlaylist.CreateFromEntity(targetPlaylist));
    }

    private async Task<PlaylistResponse> GetUserPlaylists(int userId)
    {
        List<GamePlaylist> playlists = (await this.database.Playlists.Where(p => p.CreatorId == userId)
            .ToListAsync()).ToSerializableList(GamePlaylist.CreateFromEntity);
        int total = this.database.Playlists.Count(p => p.CreatorId == userId);

        return new PlaylistResponse
        {
            Playlists = playlists,
            Total = total,
            HintStart = total+1,
        };
    }

    [HttpPost("playlists")]
    public async Task<IActionResult> CreatePlaylist()
    {
        GameTokenEntity token = this.GetToken();

        int playlistCount = await this.database.Playlists.CountAsync(p => p.CreatorId == token.UserId);

        if (playlistCount > ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota) return this.BadRequest();

        GamePlaylist? playlist = await this.DeserializeBody<GamePlaylist>("playlist");

        if (playlist == null) return this.BadRequest();

        PlaylistEntity playlistEntity = new()
        {
            CreatorId = token.UserId,
            Description = playlist.Description,
            Name = playlist.Name,
            SlotIds = playlist.SlotIds,
        };

        this.database.Playlists.Add(playlistEntity);

        await this.database.SaveChangesAsync();

        return this.Ok(GamePlaylist.CreateFromEntity(playlistEntity));
    }

    [HttpGet("user/{username}/playlists")]
    public async Task<IActionResult> GetUserPlaylists(string username)
    {
        int targetUserId = await this.database.UserIdFromUsername(username);
        if (targetUserId == 0) return this.BadRequest();

        return this.Ok(await this.GetUserPlaylists(targetUserId));
    }
}