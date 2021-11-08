#nullable enable
using System;
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
    public class SlotsController : ControllerBase
    {
        private readonly Database database;
        public SlotsController(Database database)
        {
            this.database = database;
        }

        [HttpGet("slots/by")]
        public async Task<IActionResult> SlotsBy([FromQuery] string u)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            string response = Enumerable.Aggregate
            (
                this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                    .Include(s => s.Creator)
                    .Include(s => s.Location)
                    .Where(s => s.Creator.Username == u),
                string.Empty,
                (current, slot) => current + slot.Serialize()
            );

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", 1));
        }

        [HttpGet("s/user/{id:int}")]
        public async Task<IActionResult> SUser(int id)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            Slot slot = await this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null) return this.NotFound();

            return this.Ok(slot.Serialize());
        }

        [HttpGet("slots/cool")]
        public async Task<IActionResult> CoolSlots([FromQuery] int page) => await NewestSlots(30 * page, 30);

        [HttpGet("slots")]
        public async Task<IActionResult> NewestSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            IQueryable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .OrderByDescending(s => s.FirstUploaded)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));
            string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }

        [HttpGet("slots/mmpicks")]
        public async Task<IActionResult> TeamPickedSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            IQueryable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Where(s => s.TeamPick)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .OrderByDescending(s => s.LastUpdated)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));
            string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }

        [HttpGet("slots/lbp2luckydip")]
        public async Task<IActionResult> LuckyDipSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] int seed)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            // TODO: Incorporate seed?
            IQueryable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .OrderBy(_ => Guid.NewGuid())
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));
            string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }

        
    }
}