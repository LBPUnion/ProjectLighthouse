#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Slots;

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
    public async Task<IActionResult> GetLevelQueue(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<QueuedLevel> queuedLevels = this.database.QueuedLevels.Include(q => q.User)
            .Include(q => q.Slot)
            .Include(q => q.Slot.Location)
            .Include(q => q.Slot.Creator)
            .Where(q => q.Slot.GameVersion <= gameVersion)
            .Where(q => q.User.Username == username)
            .OrderByDescending(q => q.Timestamp)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .AsEnumerable();

        string response = queuedLevels.Aggregate(string.Empty, (current, q) => current + q.Slot.Serialize(gameVersion));

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

        // don't include story levels in the 1 slot preview because otherwise invalid story levels could cause the slot to not display
        bool isProfilePreview = pageSize == 1 && pageStart == 1;

        List<HeartedLevel> heartedLevels = await this.database.HeartedLevels.Include(q => q.User)
            .Where(q => q.User.Username == username)
            .Where(q => !isProfilePreview || q.SlotType == SlotType.User)
            .OrderByDescending(q => q.Timestamp)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();
        StringBuilder responseBuilder = new();
        foreach (HeartedLevel level in heartedLevels)
        {
            if (level.SlotType == SlotType.User)
            {
                Slot? slot = await this.database.Slots.Include(s => s.Location)
                    .Include(s => s.Creator)
                    .Where(s => level.SlotId == s.SlotId)
                    .Where(s => s.GameVersion <= gameVersion)
                    .FirstOrDefaultAsync();
                responseBuilder.Append(slot?.Serialize());
            }
            else
            {
                string devSlot = await SlotTypeHelper.SerializeDeveloperSlot(this.database, level.SlotId);
                responseBuilder.Append(devSlot);
            }
        }

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
                ("favouriteSlots", responseBuilder.ToString(), "total", heartedLevels.Count)
        );
    }

    [HttpPost("favourite/slot/{levelType}/{id:int}")]
    public async Task<IActionResult> AddFavouriteSlot(string levelType, int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        SlotType slotType = SlotTypeHelper.ParseSlotType(levelType);
        if (slotType == SlotType.Unknown) return this.BadRequest();

        if (slotType == SlotType.User)
        {
            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
            if (slot == null) return this.NotFound();
        }

        await this.database.HeartLevel(user, id, slotType);

        return this.Ok();
    }

    [HttpPost("unfavourite/slot/{levelType}/{id:int}")]
    public async Task<IActionResult> RemoveFavouriteSlot(string levelType, int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        SlotType slotType = SlotTypeHelper.ParseSlotType(levelType);
        if (slotType == SlotType.Unknown) return this.BadRequest();

        if (slotType == SlotType.User)
        {
            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
            if (slot == null) return this.NotFound();
        }

        await this.database.UnheartLevel(user, id, slotType);

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