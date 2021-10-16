#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class LevelQueueController : ControllerBase {
        [HttpGet("slots/lolcatftw/{username}")]
        public IActionResult GetLevelQueue(string username) {
            IEnumerable<QueuedLevel> queuedLevels = new Database().QueuedLevels
                .Include(q => q.User)
                .Include(q => q.Slot)
                .Where(q => q.User.Username == username)
                .AsEnumerable();

            string response = queuedLevels.Aggregate(string.Empty, (current, q) => current + q.Slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }

        [HttpPost("lolcatftw/remove/user/{id:int}")]
        public async Task<IActionResult> RemoveQueuedLevel(int id) {
            await using Database database = new();

            User? user = await database.UserFromRequest(this.Request);
            if(user == null) return this.StatusCode(403, "");

            QueuedLevel queuedLevel = await database.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if(queuedLevel != null) database.QueuedLevels.Remove(queuedLevel);

            await database.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPost("lolcatftw/add/user/{id:int}")]
        public async Task<IActionResult> AddQueuedLevel(int id) {
            await using Database database = new();

            User? user = await database.UserFromRequest(this.Request);
            if(user == null) return this.StatusCode(403, "");

            QueuedLevel queuedLevel = await database.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == id);
            if(queuedLevel != null) return this.Ok();

            database.QueuedLevels.Add(new QueuedLevel {
                SlotId = id,
                UserId = user.UserId
            });

            await database.SaveChangesAsync();

            return this.Ok();
        }
    }
}