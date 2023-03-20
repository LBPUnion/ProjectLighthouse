#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
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
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Slot> queuedLevels = this.filterListByRequest(gameFilterType, dateFilterType, token.GameVersion, username, ListFilterType.Queue)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = queuedLevels.Aggregate(string.Empty, (current, q) => current + q.Serialize(gameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement("slots", response, new Dictionary<string, object>
            {
                { "total", await this.database.QueuedLevels.CountAsync(q => q.UserId == token.UserId) },
                { "hint_start", pageStart + Math.Min(pageSize, 30) },
            })
        );
    }

    [HttpPost("lolcatftw/add/user/{id:int}")]
    public async Task<IActionResult> AddQueuedLevel(int id)
    {
        GameToken token = this.GetToken();

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.QueueLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/remove/user/{id:int}")]
    public async Task<IActionResult> RemoveQueuedLevel(int id)
    {
        GameToken token = this.GetToken();

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.UnqueueLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/clear")]
    public async Task<IActionResult> ClearQueuedLevels()
    {
        GameToken token = this.GetToken();

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
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        User? targetUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) return this.StatusCode(403, "");

        IEnumerable<Slot> heartedLevels = this.filterListByRequest(gameFilterType, dateFilterType, token.GameVersion, username, ListFilterType.FavouriteSlots)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = heartedLevels.Aggregate(string.Empty, (current, q) => current + q.Serialize(gameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement("favouriteSlots", response, new Dictionary<string, object>
            {
                { "total", await this.database.HeartedLevels.CountAsync(q => q.UserId == targetUser.UserId) },
                { "hint_start", pageStart + Math.Min(pageSize, 30) },
            })
        );
    }

    private const int FirstLbp2DeveloperSlotId = 124806; // This is the first known level slot GUID in LBP2. Feel free to change it if a lower one is found.

    [HttpPost("favourite/slot/{slotType}/{id:int}")]
    public async Task<IActionResult> AddFavouriteSlot(string slotType, int id)
    {
        GameToken token = this.GetToken();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
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
        GameToken token = this.GetToken();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
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
        if (targetUserId == 0) return this.StatusCode(403, "");

        IEnumerable<Playlist> heartedPlaylists = this.database.HeartedPlaylists.Where(p => p.UserId == targetUserId)
            .Include(p => p.Playlist).Include(p => p.Playlist.Creator).OrderByDescending(p => p.HeartedPlaylistId).Select(p => p.Playlist);

        string response = heartedPlaylists.Aggregate(string.Empty, (current, p) => current + p.Serialize());

        return this.Ok
        (
            LbpSerializer.TaggedStringElement("favouritePlaylists", response, new Dictionary<string, object>
            {
                { "total", this.database.HeartedPlaylists.Count(p => p.UserId == targetUserId) },
                { "hint_start", pageStart + Math.Min(pageSize, 30) },
            })
        );
    }

    [HttpPost("favourite/playlist/{playlistId:int}")]
    public async Task<IActionResult> AddFavouritePlaylist(int playlistId)
    {
        GameToken token = this.GetToken();

        Playlist? playlist = await this.database.Playlists.FirstOrDefaultAsync(s => s.PlaylistId == playlistId);
        if (playlist == null) return this.NotFound();

        await this.database.HeartPlaylist(token.UserId, playlist);

        return this.Ok();
    }

    [HttpPost("unfavourite/playlist/{playlistId:int}")]
    public async Task<IActionResult> RemoveFavouritePlaylist(int playlistId)
    {
        GameToken token = this.GetToken();

        Playlist? playlist = await this.database.Playlists.FirstOrDefaultAsync(s => s.PlaylistId == playlistId);
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
        GameToken token = this.GetToken();

        User? targetUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        IEnumerable<User> heartedProfiles = this.database.HeartedProfiles.Include
                (q => q.HeartedUser)
            .OrderBy(q => q.HeartedProfileId)
            .Where(q => q.UserId == targetUser.UserId)
            .Select(q => q.HeartedUser)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = heartedProfiles.Aggregate(string.Empty, (current, u) => current + u.Serialize(token.GameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement("favouriteUsers", response, new Dictionary<string, object>
            {
                { "total", await this.database.HeartedProfiles.CountAsync(q => q.UserId == targetUser.UserId) },
                { "hint_start", pageStart + Math.Min(pageSize, 30) },
            })
        );
    }

    [HttpPost("favourite/user/{username}")]
    public async Task<IActionResult> AddFavouriteUser(string username)
    {
        GameToken token = this.GetToken();

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(token.UserId, heartedUser);

        return this.Ok();
    }

    [HttpPost("unfavourite/user/{username}")]
    public async Task<IActionResult> RemoveFavouriteUser(string username)
    {
        GameToken token = this.GetToken();

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(token.UserId, heartedUser);

        return this.Ok();
    }

    #endregion

    #region Filtering
    enum ListFilterType // used to collapse code that would otherwise be two separate functions
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

    private IQueryable<Slot> filterListByRequest(string? gameFilterType, string? dateFilterType, GameVersion version, string username, ListFilterType filterType)
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
            IQueryable<QueuedLevel> whereQueuedLevels;

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

        IQueryable<HeartedLevel> whereHeartedLevels;

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
