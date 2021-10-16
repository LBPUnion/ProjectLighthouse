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
    public class SlotsController : ControllerBase {
        [HttpGet("slots/by")]
        public IActionResult SlotsBy() {
            string response = Enumerable.Aggregate(new Database().Slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }

        [HttpGet("s/user/{id:int}")]
        public IActionResult SUser(int id) {
            IEnumerable<Slot> slots = new Database().Slots
                .Where(s => s.CreatorId == id)
                .AsEnumerable();

            string response = slots.Aggregate(string.Empty, (current, s) => current + s.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }
    }
}