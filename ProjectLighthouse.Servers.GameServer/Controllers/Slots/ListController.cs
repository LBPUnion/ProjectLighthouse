#nullable enable
using LBPUnion.ProjectLighthouse.Helpers;
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

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Slot> queuedLevels = this.database.QueuedLevels.Where(q => q.User.Username == username)
            .Select(q => q.Slot)
            .ByGameVersion(gameVersion, false, true)
            .Skip(pageStart - 1)
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
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.QueueLevel(user, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/remove/user/{id:int}")]
    public async Task<IActionResult> RemoveQueuedLevel(int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.UnqueueLevel(user, slot);

        return this.Ok();
    }

    [HttpPost("lolcatftw/clear")]
    public async Task<IActionResult> ClearQueuedLevels()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        this.database.QueuedLevels.RemoveRange(this.database.QueuedLevels.Where(q => q.UserId == user.UserId));

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

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Slot> heartedLevels = this.database.HeartedLevels.Where(q => q.User.Username == username)
            .Select(q => q.Slot)
            .ByGameVersion(gameVersion, false, true)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = heartedLevels.Aggregate(string.Empty, (current, q) => current + q.Serialize(gameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
                ("favouriteSlots", response, "total", this.database.HeartedLevels.Include(q => q.User).Count(q => q.User.Username == username))
        );
    }

    [HttpPost("favourite/slot/user/{id:int}")]
    public async Task<IActionResult> AddFavouriteSlot(int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.HeartLevel(user, slot);

        return this.Ok();
    }

    [HttpPost("unfavourite/slot/user/{id:int}")]
    public async Task<IActionResult> RemoveFavouriteSlot(int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        await this.database.UnheartLevel(user, slot);

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

        IEnumerable<HeartedProfile> heartedProfiles = this.database.HeartedProfiles.Include
                (q => q.User)
            .Include(q => q.HeartedUser)
            .Include(q => q.HeartedUser.Location)
            .Where(q => q.User.Username == username)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = heartedProfiles.Aggregate(string.Empty, (current, q) => current + q.HeartedUser.Serialize(token.GameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
                ("favouriteUsers", response, "total", this.database.HeartedProfiles.Include(q => q.User).Count(q => q.User.Username == username))
        );
    }

    [HttpPost("favourite/user/{username}")]
    public async Task<IActionResult> AddFavouriteUser(string username)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(user, heartedUser);

        return this.Ok();
    }

    [HttpPost("unfavourite/user/{username}")]
    public async Task<IActionResult> RemoveFavouriteUser(string username)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(user, heartedUser);

        return this.Ok();
    }

    #endregion

}