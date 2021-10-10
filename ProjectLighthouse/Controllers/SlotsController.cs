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
        public async Task<IActionResult> SUser(int id) {
            Slot slot = await new Database().Slots.FirstOrDefaultAsync(s => s.SlotId == id);

            return this.Ok(slot.Serialize());
        }

        [HttpGet("slots/lolcatftw/{username}")]
        public IActionResult SlotsLolCat(string username) {
            string response = Enumerable.Aggregate(new Database().Slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }
    }
}