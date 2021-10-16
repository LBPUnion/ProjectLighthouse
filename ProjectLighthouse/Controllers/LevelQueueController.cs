using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }
}