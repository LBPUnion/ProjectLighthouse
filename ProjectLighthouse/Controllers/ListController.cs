#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class ListController : ControllerBase {
        private readonly Database database;
        public ListController(Database database) {
            this.database = database;
        }

        #region Levels
        #region Level Queue (lolcatftw)

        [HttpGet("slots/lolcatftw/{username}")]
        public IActionResult GetLevelQueue(string username) {
            IEnumerable<QueuedLevel> queuedLevels = new Database().QueuedLevels
                .Include(q => q.User)
                .Include(q => q.Slot)
                .Include(q => q.Slot.Location)
                .Where(q => q.User.Username == username)
                .AsEnumerable();

            string response = queuedLevels.Aggregate(string.Empty, (current, q) => current + q.Slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }

        [HttpPost("lolcatftw/add/user/{id:int}")]
        public async Task<IActionResult> AddQueuedLevel(int id) {
            User? user = await this.database.UserFromRequest(this.Request);
            if(user == null) return this.StatusCode(403, "");

            QueuedLevel queuedLevel = await this.database.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if(queuedLevel != null) return this.Ok();

            this.database.QueuedLevels.Add(new QueuedLevel {
                SlotId = id,
                UserId = user.UserId,
            });

            await this.database.SaveChangesAsync();

            return this.Ok();
        }
        
        [HttpPost("lolcatftw/remove/user/{id:int}")]
        public async Task<IActionResult> RemoveQueuedLevel(int id) {
            User? user = await this.database.UserFromRequest(this.Request);
            if(user == null) return this.StatusCode(403, "");

            QueuedLevel queuedLevel = await this.database.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if(queuedLevel != null) this.database.QueuedLevels.Remove(queuedLevel);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        #endregion

        #region Hearted Levels

        [HttpGet("favouriteSlots/{username}")]
        public IActionResult GetFavouriteSlots(string username) {
            IEnumerable<HeartedLevel> heartedLevels = new Database().HeartedLevels
                .Include(q => q.User)
                .Include(q => q.Slot)
                .Include(q => q.Slot.Location)
                .Include(q => q.Slot.Creator)
                .Where(q => q.User.Username == username)
                .AsEnumerable();

            string response = heartedLevels.Aggregate(string.Empty, (current, q) => current + q.Slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("favouriteSlots", response, "total", 1));
        }

        [HttpPost("favourite/slot/user/{id:int}")]
        public async Task<IActionResult> AddFavouriteSlot(int id) {
            User? user = await this.database.UserFromRequest(this.Request);
            if(user == null) return this.StatusCode(403, "");

            HeartedLevel heartedLevel = await this.database.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if(heartedLevel != null) return this.Ok();

            this.database.HeartedLevels.Add(new HeartedLevel {
                SlotId = id,
                UserId = user.UserId,
            });

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPost("unfavourite/slot/user/{id:int}")]
        public async Task<IActionResult> RemoveFavouriteSlot(int id) {
            User? user = await this.database.UserFromRequest(this.Request);
            if(user == null) return this.StatusCode(403, "");

            HeartedLevel heartedLevel = await this.database.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if(heartedLevel != null) this.database.HeartedLevels.Remove(heartedLevel);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        #endregion
        #endregion Levels

        #region Users

        

        #endregion
    }
}