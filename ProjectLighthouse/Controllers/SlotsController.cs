using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class SlotsController : ControllerBase
    {
        private readonly Database database;
        public SlotsController(Database database)
        {
            this.database = database;
        }

        [HttpGet("slots/by")]
        public IActionResult SlotsBy([FromQuery] string u)
        {
            string response = Enumerable.Aggregate
            (
                this.database.Slots.Include(s => s.Creator).Include(s => s.Location).Where(s => s.Creator.Username == u),
                string.Empty,
                (current, slot) => current + slot.Serialize()
            );

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }

        [HttpGet("s/user/{id:int}")]
        public async Task<IActionResult> SUser(int id)
        {
            Slot slot = await this.database.Slots.Include(s => s.Creator).Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null) return this.NotFound();

            return this.Ok(slot.Serialize());
        }

        [HttpGet("slots")]
        public IActionResult NewestSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            IQueryable<Slot> slots = this.database.Slots.Include
                    (s => s.Creator)
                .Include(s => s.Location)
                .OrderByDescending(s => s.FirstUploaded)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));
            string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }
    }
}