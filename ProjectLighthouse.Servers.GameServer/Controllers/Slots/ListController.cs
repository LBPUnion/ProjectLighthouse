#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ListController : ControllerBase
{
    private readonly DatabaseContext database;

    public ListController(DatabaseContext database)
    {
        this.database = database;
    }

    #region Levels

    #region Level Queue (lolcatftw)

    [HttpGet("slots/lolcatftw/{username}")]
    public async Task<IActionResult> GetQueuedLevels(string username)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        int targetUserId = await this.database.Users.Where(u => u.Username == username)
            .Select(u => u.UserId)
            .FirstOrDefaultAsync();
        if (targetUserId == 0) return this.BadRequest();

        pageData.TotalElements = await this.database.QueuedLevels.CountAsync(q => q.UserId == targetUserId);

        IQueryable<SlotEntity> baseQuery = this.database.QueuedLevels.Where(h => h.UserId == targetUserId)
            .OrderByDescending(q => q.QueuedLevelId)
            .Include(q => q.Slot)
            .Select(q => q.Slot);

        List<SlotBase> queuedLevels = await baseQuery.GetSlots(token,
            this.FilterFromRequest(token),
            pageData,
            new SlotSortBuilder<SlotEntity>());

        return this.Ok(new GenericSlotResponse(queuedLevels, pageData));
    }

    [HttpPost("lolcatftw/add/user/{id:int}")]
    public async Task<IActionResult> AddQueuedLevel(int id)
    {
        GameTokenEntity token = this.GetToken();

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.QueueLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/remove/user/{id:int}")]
    public async Task<IActionResult> RemoveQueuedLevel(int id)
    {
        GameTokenEntity token = this.GetToken();

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.UnqueueLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/clear")]
    public async Task<IActionResult> ClearQueuedLevels()
    {
        GameTokenEntity token = this.GetToken();

        this.database.QueuedLevels.RemoveRange(this.database.QueuedLevels.Where(q => q.UserId == token.UserId));

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    #endregion

    #region Hearted Levels

    [HttpGet("favouriteSlots/{username}")]
    public async Task<IActionResult> GetFavouriteSlots(string username)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        int targetUserId = await this.database.Users.Where(u => u.Username == username)
            .Select(u => u.UserId)
            .FirstOrDefaultAsync();
        if (targetUserId == 0) return this.BadRequest();

        pageData.TotalElements = await this.database.HeartedLevels.CountAsync(h => h.UserId == targetUserId);

        IQueryable<SlotEntity> baseQuery = this.database.HeartedLevels.Where(h => h.UserId == targetUserId)
            .OrderByDescending(h => h.HeartedLevelId)
            .Include(h => h.Slot)
            .Select(h => h.Slot);

        List<SlotBase> heartedLevels = await baseQuery.GetSlots(token,
            this.FilterFromRequest(token),
            pageData,
            new SlotSortBuilder<SlotEntity>());

        return this.Ok(new GenericSlotResponse("favouriteSlots", heartedLevels, pageData));
    }

    private const int firstLbp2DeveloperSlotId = 124806; // This is the first known level slot GUID in LBP2. Feel free to change it if a lower one is found.

    [HttpPost("favourite/slot/{slotType}/{id:int}")]
    public async Task<IActionResult> AddFavouriteSlot(string slotType, int id)
    {
        GameTokenEntity token = this.GetToken();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        if (slotType == "developer")
        {
            GameVersion slotGameVersion = (slot.InternalSlotId < firstLbp2DeveloperSlotId) ? GameVersion.LittleBigPlanet1 : token.GameVersion;
            slot.GameVersion = slotGameVersion;
        }

        await this.database.HeartLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("unfavourite/slot/{slotType}/{id:int}")]
    public async Task<IActionResult> RemoveFavouriteSlot(string slotType, int id)
    {
        GameTokenEntity token = this.GetToken();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        if (slotType == "developer")
        {
            GameVersion slotGameVersion = (slot.InternalSlotId < firstLbp2DeveloperSlotId) ? GameVersion.LittleBigPlanet1 : token.GameVersion;
            slot.GameVersion = slotGameVersion;
        }
        
        await this.database.UnheartLevel(token.UserId, slot);

        return this.Ok();
    }

    #endregion

    #region Hearted Playlists

    [HttpGet("favouritePlaylists/{username}")]
    public async Task<IActionResult> GetFavouritePlaylists(string username)
    {

        int targetUserId = await this.database.UserIdFromUsername(username);
        if (targetUserId == 0) return this.Forbid();

        PaginationData pageData = this.Request.GetPaginationData();

        List<GamePlaylist> heartedPlaylists = (await this.database.HeartedPlaylists.Where(p => p.UserId == targetUserId)
            .Include(p => p.Playlist)
            .OrderByDescending(p => p.HeartedPlaylistId)
            .Select(p => p.Playlist)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(GamePlaylist.CreateFromEntity);

        pageData.TotalElements = await this.database.HeartedPlaylists.CountAsync(p => p.UserId == targetUserId);

        return this.Ok(new GenericPlaylistResponse<GamePlaylist>("favouritePlaylists", heartedPlaylists)
        {
            Total = pageData.TotalElements,
            HintStart = pageData.HintStart,
        });
    }

    [HttpPost("favourite/playlist/{playlistId:int}")]
    public async Task<IActionResult> AddFavouritePlaylist(int playlistId)
    {
        GameTokenEntity token = this.GetToken();

        PlaylistEntity? playlist = await this.database.Playlists.FirstOrDefaultAsync(s => s.PlaylistId == playlistId);
        if (playlist == null) return this.NotFound();

        await this.database.HeartPlaylist(token.UserId, playlist);

        return this.Ok();
    }

    [HttpPost("unfavourite/playlist/{playlistId:int}")]
    public async Task<IActionResult> RemoveFavouritePlaylist(int playlistId)
    {
        GameTokenEntity token = this.GetToken();

        PlaylistEntity? playlist = await this.database.Playlists.FirstOrDefaultAsync(s => s.PlaylistId == playlistId);
        if (playlist == null) return this.NotFound();

        await this.database.UnheartPlaylist(token.UserId, playlist);

        return this.Ok();
    }

    #endregion

    #endregion Levels

    #region Users

    [HttpGet("favouriteUsers/{username}")]
    public async Task<IActionResult> GetFavouriteUsers(string username)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        int targetUserId = await this.database.Users.Where(u => u.Username == username)
            .Select(u => u.UserId)
            .FirstOrDefaultAsync();
        if (targetUserId == 0) return this.BadRequest();

        pageData.TotalElements = await this.database.HeartedProfiles.CountAsync(h => h.UserId == targetUserId);

        List<GameUser> heartedProfiles = (await this.database.HeartedProfiles.Include(h => h.HeartedUser)
            .OrderByDescending(h => h.HeartedProfileId)
            .Where(h => h.UserId == targetUserId)
            .Select(h => h.HeartedUser)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(u => GameUser.CreateFromEntity(u, token.GameVersion));

        return this.Ok(new GenericUserResponse<GameUser>("favouriteUsers", heartedProfiles, pageData));
    }

    [HttpPost("favourite/user/{username}")]
    public async Task<IActionResult> AddFavouriteUser(string username)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(token.UserId, heartedUser);

        return this.Ok();
    }

    [HttpPost("unfavourite/user/{username}")]
    public async Task<IActionResult> RemoveFavouriteUser(string username)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(token.UserId, heartedUser);

        return this.Ok();
    }
    #endregion
}
