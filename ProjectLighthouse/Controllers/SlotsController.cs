using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types.Levels;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class SlotsController : ControllerBase {
        private readonly Database database;
        public SlotsController(Database database) {
            this.database = database;
        }

        [HttpGet("slots/by")]
        public IActionResult SlotsBy([FromQuery] string u) {
            string response = Enumerable.Aggregate(
                database.Slots
                    .Include(s => s.Creator)
                    .Include(s => s.Location)
                    .Where(s => s.Creator.Username == u)
                , string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }
        
        public async Task<IActionResult> SUser(int id) {
            Slot slot = await this.database.Slots
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            return this.Ok(slot.Serialize());
        }
    }
}