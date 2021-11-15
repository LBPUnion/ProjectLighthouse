#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
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
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            IEnumerable<QueuedLevel> queuedLevels = this.database.QueuedLevels.Include(q => q.User)
                .Include(q => q.Slot)
                .Include(q => q.Slot.Location)
                .Include(q => q.Slot.Creator)
                .Where(q => q.Slot.GameVersion <= gameVersion)
                .Where(q => q.User.Username == username)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30))
                .AsEnumerable();

            string response = queuedLevels.Aggregate(string.Empty, (current, q) => current + q.Slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", this.database.QueuedLevels.Include(q => q.User).Where(q => q.User.Username == username).Count()));
        }

        [HttpPost("lolcatftw/add/user/{id:int}")]
        public async Task<IActionResult> AddQueuedLevel(int id)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            QueuedLevel queuedLevel = await this.database.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if (queuedLevel != null) return this.Ok();

            this.database.QueuedLevels.Add
            (
                new QueuedLevel
                {
                    SlotId = id,
                    UserId = user.UserId,
                }
            );

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPost("lolcatftw/remove/user/{id:int}")]
        public async Task<IActionResult> RemoveQueuedLevel(int id)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            QueuedLevel queuedLevel = await this.database.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if (queuedLevel != null) this.database.QueuedLevels.Remove(queuedLevel);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        #endregion

        #region Hearted Levels

        [HttpGet("favouriteSlots/{username}")]
        public async Task<IActionResult> GetFavouriteSlots(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            IEnumerable<HeartedLevel> heartedLevels = this.database.HeartedLevels.Include(q => q.User)
                .Include(q => q.Slot)
                .Include(q => q.Slot.Location)
                .Include(q => q.Slot.Creator)
                .Where(q => q.Slot.GameVersion <= gameVersion)
                .Where(q => q.User.Username == username)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30))
                .AsEnumerable();

            string response = heartedLevels.Aggregate(string.Empty, (current, q) => current + q.Slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("favouriteSlots", response, "total", this.database.HeartedLevels.Include(q => q.User).Where(q => q.User.Username == username).Count()));
        }

        [HttpPost("favourite/slot/user/{id:int}")]
        public async Task<IActionResult> AddFavouriteSlot(int id)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            HeartedLevel heartedLevel = await this.database.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if (heartedLevel != null) return this.Ok();

            this.database.HeartedLevels.Add
            (
                new HeartedLevel
                {
                    SlotId = id,
                    UserId = user.UserId,
                }
            );

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPost("unfavourite/slot/user/{id:int}")]
        public async Task<IActionResult> RemoveFavouriteSlot(int id)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            HeartedLevel heartedLevel = await this.database.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if (heartedLevel != null) this.database.HeartedLevels.Remove(heartedLevel);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        #endregion

        #endregion Levels

        #region Users

        [HttpGet("favouriteUsers/{username}")]
        public async Task<IActionResult> GetFavouriteUsers(string username, [FromQuery] int pageSize, [FromQuery] int pageStart)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
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

            return this.Ok(LbpSerializer.TaggedStringElement("favouriteUsers", response, "total", this.database.HeartedProfiles.Include(q => q.User).Where(q => q.User.Username == username).Count()));
        }

        [HttpPost("favourite/user/{username}")]
        public async Task<IActionResult> AddFavouriteUser(string username)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (heartedUser == null) return this.NotFound();

            HeartedProfile heartedProfile = await this.database.HeartedProfiles.FirstOrDefaultAsync
                (q => q.UserId == user.UserId && q.HeartedUserId == heartedUser.UserId);
            if (heartedProfile != null) return this.Ok();

            this.database.HeartedProfiles.Add
            (
                new HeartedProfile
                {
                    HeartedUserId = heartedUser.UserId,
                    UserId = user.UserId,
                }
            );

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPost("unfavourite/user/{username}")]
        public async Task<IActionResult> RemoveFavouriteUser(string username)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (heartedUser == null) return this.NotFound();

            HeartedProfile heartedProfile = await this.database.HeartedProfiles.FirstOrDefaultAsync
                (q => q.UserId == user.UserId && q.HeartedUserId == heartedUser.UserId);
            if (heartedProfile != null) this.database.HeartedProfiles.Remove(heartedProfile);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        #endregion

    }
}
