#nullable enable
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ListController : ControllerBase
{
    private readonly Database database;
    public ListController(Database database)
    {
        this.database = database;
    }

    #region Levels

    #region Level Queue (lolcatftw)

    [HttpGet("slots/lolcatftw/{username}")]
    public async Task<IActionResult> GetQueuedLevels(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Slot> queuedLevels = this.database.QueuedLevels.Where(q => q.User.Username == username)
            .Include(q => q.Slot.Creator)
            .Include(q => q.Slot.Location)
            .Select(q => q.Slot)
            .ByGameVersion(gameVersion)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = queuedLevels.Aggregate(string.Empty, (current, q) => current + q.Serialize(gameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
                ("slots", response, "total", this.database.QueuedLevels.Include(q => q.User).Count(q => q.User.Username == username))
        );
    }

    [HttpPost("lolcatftw/add/user/{id:int}")]
    public async Task<IActionResult> AddQueuedLevel(int id)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.QueueLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/remove/user/{id:int}")]
    public async Task<IActionResult> RemoveQueuedLevel(int id)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.UnqueueLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/clear")]
    public async Task<IActionResult> ClearQueuedLevels()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        this.database.QueuedLevels.RemoveRange(this.database.QueuedLevels.Where(q => q.UserId == token.UserId));

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    #endregion

    #region Hearted Levels

    [HttpGet("favouriteSlots/{username}")]
    public async Task<IActionResult> GetFavouriteSlots(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        User? targetUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) return this.StatusCode(403, "");

        IEnumerable<Slot> heartedLevels = this.database.HeartedLevels.Where(q => q.UserId == targetUser.UserId)
            .Include(q => q.Slot.Creator)
            .Include(q => q.Slot.Location)
            .Select(q => q.Slot)
            .ByGameVersion(gameVersion)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = heartedLevels.Aggregate(string.Empty, (current, q) => current + q.Serialize(gameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement("favouriteSlots", response, new Dictionary<string, object>
            {
                { "total", this.database.HeartedLevels.Count(q => q.UserId == targetUser.UserId) },
                { "hint_start", pageStart + Math.Min(pageSize, 30) },
            })
        );
    }

    [HttpPost("favourite/slot/user/{id:int}")]
    public async Task<IActionResult> AddFavouriteSlot(int id)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.HeartLevel(token.UserId, slot);

        return this.Ok();
    }

    [HttpPost("unfavourite/slot/user/{id:int}")]
    public async Task<IActionResult> RemoveFavouriteSlot(int id)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.UnheartLevel(token.UserId, slot);

        return this.Ok();
    }

    #endregion

    #endregion Levels

    #region Users

    [HttpGet("favouriteUsers/{username}")]
    public async Task<IActionResult> GetFavouriteUsers(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        User? targetUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        IEnumerable<User> heartedProfiles = this.database.HeartedProfiles.Include
                (q => q.HeartedUser)
            .Include(q => q.HeartedUser.Location)
            .Select(q => q.HeartedUser)
            .Where(q => q.UserId == targetUser.UserId)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = heartedProfiles.Aggregate(string.Empty, (current, u) => current + u.Serialize(token.GameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement("favouriteUsers", response, new Dictionary<string, object>
            {
                { "total", this.database.HeartedProfiles.Count(q => q.UserId == targetUser.UserId) },
                { "hint_start", pageStart + Math.Min(pageSize, 30) },
            })
        );
    }

    [HttpPost("favourite/user/{username}")]
    public async Task<IActionResult> AddFavouriteUser(string username)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(token.UserId, heartedUser);

        return this.Ok();
    }

    [HttpPost("unfavourite/user/{username}")]
    public async Task<IActionResult> RemoveFavouriteUser(string username)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(token.UserId, heartedUser);

        return this.Ok();
    }

    #endregion

}