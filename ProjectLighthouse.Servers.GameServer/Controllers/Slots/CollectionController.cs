#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CollectionController : ControllerBase
{
    private readonly DatabaseContext database;

    public CollectionController(DatabaseContext database)
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

        return this.Ok(await this.GetUserPlaylists(token.UserId));
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

    [HttpGet("searches")]
    [HttpGet("genres")]
    public async Task<IActionResult> GenresAndSearches()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        List<GameCategory> categories = new();

        foreach (Category category in CategoryHelper.Categories.ToList())
        {
            if(category is CategoryWithUser categoryWithUser) categories.Add(categoryWithUser.Serialize(this.database, user));
            else categories.Add(category.Serialize(this.database));
        }

        return this.Ok(new CategoryListResponse(categories, CategoryHelper.Categories.Count, 0, 1));
    }

    [HttpGet("searches/{endpointName}")]
    public async Task<IActionResult> GetCategorySlots(string endpointName, [FromQuery] int pageStart, [FromQuery] int pageSize,
        [FromQuery] int players = 0,
        [FromQuery] string? labelFilter0 = null,
        [FromQuery] string? labelFilter1 = null,
        [FromQuery] string? labelFilter2 = null,
        [FromQuery] string? labelFilter3 = null,
        [FromQuery] string? labelFilter4 = null,
        [FromQuery] string? move = null,
        [FromQuery] string? adventure = null
    )
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        Category? category = CategoryHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
        if (category == null) return this.NotFound();

        Logger.Debug("Found category " + category, LogArea.Category);

        List<SlotEntity> slots;
        int totalSlots;

        if (category is CategoryWithUser categoryWithUser)
        {
            slots = (await categoryWithUser.GetSlots(this.database, user, pageStart, pageSize)
                .ToListAsync());
            totalSlots = categoryWithUser.GetTotalSlots(this.database, user);
        }
        else
        {
            slots = category.GetSlots(this.database, pageStart, pageSize)
                .ToList();
            totalSlots = category.GetTotalSlots(this.database);
        }

        slots = this.filterSlots(slots, players + 1, labelFilter0, labelFilter1, labelFilter2, labelFilter3, labelFilter4, move, adventure);

        return this.Ok(new GenericSlotResponse("results", slots.ToSerializableList(s => SlotBase.CreateFromEntity(s, token)), totalSlots, pageStart + pageSize));
    }

    private List<SlotEntity> filterSlots(List<SlotEntity> slots, int players, string? labelFilter0 = null, string? labelFilter1 = null, string? labelFilter2 = null, string? labelFilter3 = null, string? labelFilter4 = null, string? move = null, string? adventure = null)
    {
        slots.RemoveAll(s => s.MinimumPlayers != players);

        if (labelFilter0 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter0));
        if (labelFilter1 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter1));
        if (labelFilter2 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter2));
        if (labelFilter3 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter3));
        if (labelFilter4 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter4));

        if (move == "noneCan")
            slots.RemoveAll(s => s.MoveRequired);
        if (move == "allMust")
            slots.RemoveAll(s => !s.MoveRequired);

        if (adventure == "noneCan")
            slots.RemoveAll(s => s.IsAdventurePlanet);
        if (adventure == "allMust")
            slots.RemoveAll(s => !s.IsAdventurePlanet);

        return slots;
    }
}