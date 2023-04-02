#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
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
    public async Task<IActionResult> GetQueuedLevels
    (
        string username,
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null,
        [FromQuery] string? dateFilterType = null
    )
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        List<SlotBase> queuedLevels = (await this.filterListByRequest(gameFilterType, dateFilterType, token.GameVersion, username, ListFilterType.Queue)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync())
            .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));

        int total = await this.database.QueuedLevels.CountAsync(q => q.UserId == token.UserId);
        int start = pageStart + Math.Min(pageSize, 30);

        return this.Ok(new GenericSlotResponse(queuedLevels, total, start));
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
    public async Task<IActionResult> GetFavouriteSlots
    (
        string username,
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null,
        [FromQuery] string? dateFilterType = null
    )
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        UserEntity? targetUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) return this.Forbid();

        List<SlotBase> heartedLevels = (await this.filterListByRequest(gameFilterType, dateFilterType, token.GameVersion, username, ListFilterType.FavouriteSlots)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync()).ToSerializableList(s => SlotBase.CreateFromEntity(s, token));
            

        int total = await this.database.HeartedLevels.CountAsync(q => q.UserId == targetUser.UserId);
        int start = pageStart + Math.Min(pageSize, 30);

        return this.Ok(new GenericSlotResponse("favouriteSlots", heartedLevels, total, start));
    }

    private const int FirstLbp2DeveloperSlotId = 124806; // This is the first known level slot GUID in LBP2. Feel free to change it if a lower one is found.

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
            GameVersion slotGameVersion = (slot.InternalSlotId < FirstLbp2DeveloperSlotId) ? GameVersion.LittleBigPlanet1 : token.GameVersion;
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
            GameVersion slotGameVersion = (slot.InternalSlotId < FirstLbp2DeveloperSlotId) ? GameVersion.LittleBigPlanet1 : token.GameVersion;
            slot.GameVersion = slotGameVersion;
        }
        
        await this.database.UnheartLevel(token.UserId, slot);

        return this.Ok();
    }

    #endregion

    #region Hearted Playlists

    [HttpGet("favouritePlaylists/{username}")]
    public async Task<IActionResult> GetFavouritePlaylists(string username, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        if (pageSize <= 0) return this.BadRequest();

        int targetUserId = await this.database.UserIdFromUsername(username);
        if (targetUserId == 0) return this.Forbid();

        List<GamePlaylist> heartedPlaylists = (await this.database.HeartedPlaylists.Where(p => p.UserId == targetUserId)
            .Include(p => p.Playlist)
            .Include(p => p.Playlist.Creator)
            .OrderByDescending(p => p.HeartedPlaylistId)
            .Select(p => p.Playlist)
            .ToListAsync()).ToSerializableList(GamePlaylist.CreateFromEntity);

        int total = await this.database.HeartedPlaylists.CountAsync(p => p.UserId == targetUserId);

        return this.Ok(new GenericPlaylistResponse<GamePlaylist>("favouritePlaylists", heartedPlaylists)
        {
            Total = total,
            HintStart = pageStart + Math.Min(pageSize, 30),
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
    public async Task<IActionResult> GetFavouriteUsers(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? targetUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) return this.Forbid();

        if (pageSize <= 0) return this.BadRequest();

        List<GameUser> heartedProfiles = (await this.database.HeartedProfiles.Include(h => h.HeartedUser)
            .OrderBy(h => h.HeartedProfileId)
            .Where(h => h.UserId == targetUser.UserId)
            .Select(h => h.HeartedUser)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync()).ToSerializableList(u => GameUser.CreateFromEntity(u, token.GameVersion));

        int total = await this.database.HeartedProfiles.CountAsync(h => h.UserId == targetUser.UserId);

        return this.Ok(new GenericUserResponse<GameUser>("favouriteUsers", heartedProfiles, total, pageStart + Math.Min(pageSize, 30)));
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

    #region Filtering
    internal enum ListFilterType // used to collapse code that would otherwise be two separate functions
    {
        Queue,
        FavouriteSlots,
    }

    private static GameVersion getGameFilter(string? gameFilterType, GameVersion version)
    {
        return version switch
        {
            GameVersion.LittleBigPlanetVita => GameVersion.LittleBigPlanetVita,
            GameVersion.LittleBigPlanetPSP => GameVersion.LittleBigPlanetPSP,
            _ => gameFilterType switch
            {
                "lbp1" => GameVersion.LittleBigPlanet1,
                "lbp2" => GameVersion.LittleBigPlanet2,
                "lbp3" => GameVersion.LittleBigPlanet3,
                "both" => GameVersion.LittleBigPlanet2, // LBP2 default option
                null => GameVersion.LittleBigPlanet1,
                _ => GameVersion.Unknown,
            }
        };
    }

    private IQueryable<SlotEntity> filterListByRequest(string? gameFilterType, string? dateFilterType, GameVersion version, string username, ListFilterType filterType)
    {
        if (version is GameVersion.LittleBigPlanetPSP or GameVersion.Unknown)
        {
            return this.database.Slots.ByGameVersion(version, false, true);
        }

        long oldestTime = dateFilterType switch
        {
            "thisWeek" => DateTimeOffset.Now.AddDays(-7).ToUnixTimeMilliseconds(),
            "thisMonth" => DateTimeOffset.Now.AddDays(-31).ToUnixTimeMilliseconds(),
            _ => 0,
        };

        GameVersion gameVersion = getGameFilter(gameFilterType, version);

        // The filtering only cares if this isn't equal to 'both'
        if (version == GameVersion.LittleBigPlanetVita) gameFilterType = "lbp2";

        if (filterType == ListFilterType.Queue)
        {
            IQueryable<QueuedLevelEntity> whereQueuedLevels;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (gameFilterType == "both")
                // Get game versions less than the current version
                // Needs support for LBP3 ("both" = LBP1+2)
                whereQueuedLevels = this.database.QueuedLevels.Where(q => q.User.Username == username)
                .Where(q => q.Slot.Type == SlotType.User && !q.Slot.Hidden && q.Slot.GameVersion <= gameVersion && q.Slot.FirstUploaded >= oldestTime);
            else
                // Get game versions exactly equal to gamefiltertype
                whereQueuedLevels = this.database.QueuedLevels.Where(q => q.User.Username == username)
                .Where(q => q.Slot.Type == SlotType.User && !q.Slot.Hidden && q.Slot.GameVersion == gameVersion && q.Slot.FirstUploaded >= oldestTime);

            return whereQueuedLevels.OrderByDescending(q => q.QueuedLevelId).Include(q => q.Slot.Creator).Select(q => q.Slot).ByGameVersion(gameVersion, false, false, true);
        }

        IQueryable<HeartedLevelEntity> whereHeartedLevels;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (gameFilterType == "both")
            // Get game versions less than the current version
            // Needs support for LBP3 ("both" = LBP1+2)
            whereHeartedLevels = this.database.HeartedLevels.Where(h => h.User.Username == username)
                .Where(h => (h.Slot.Type == SlotType.User || h.Slot.Type == SlotType.Developer) && !h.Slot.Hidden && h.Slot.GameVersion <= gameVersion && h.Slot.FirstUploaded >= oldestTime);
        else
            // Get game versions exactly equal to gamefiltertype
            whereHeartedLevels = this.database.HeartedLevels.Where(h => h.User.Username == username)
                .Where(h => (h.Slot.Type == SlotType.User || h.Slot.Type == SlotType.Developer) && !h.Slot.Hidden && h.Slot.GameVersion == gameVersion && h.Slot.FirstUploaded >= oldestTime);

        return whereHeartedLevels.OrderByDescending(h => h.HeartedLevelId).Include(h => h.Slot.Creator).Select(h => h.Slot).ByGameVersion(gameVersion, false, false, true);
    }
    #endregion Filtering
}
