#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Settings;
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
        public async Task<IActionResult> SlotsBy([FromQuery] string u, [FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            User user = await this.database.Users.FirstOrDefaultAsync(dbUser => dbUser.Username == u);

            string response = Enumerable.Aggregate
            (
                this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                    .Include(s => s.Creator)
                    .Include(s => s.Location)
                    .Where(s => s.Creator.Username == user.Username)
                    .Skip(pageStart - 1)
                    .Take(Math.Min(pageSize, ServerSettings.EntitledSlots)),
                string.Empty,
                (current, slot) => current + slot.Serialize()
            );

            return this.Ok
            (
                LbpSerializer.TaggedStringElement
                (
                    "slots",
                    response,
                    new Dictionary<string, object>
                    {
                        {
                            "hint_start", pageStart + Math.Min(pageSize, ServerSettings.EntitledSlots)
                        },
                        {
                            "total", user.UsedSlots
                        },
                    }
                )
            );
        }

        [HttpGet("s/user/{id:int}")]
        public async Task<IActionResult> SUser(int id)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            Slot? slot = await this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null) return this.NotFound();

            RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == id && r.UserId == user.UserId);
            VisitedLevel? visitedLevel = await this.database.VisitedLevels.FirstOrDefaultAsync(r => r.SlotId == id && r.UserId == user.UserId);
            return this.Ok(slot.Serialize(ratedLevel, visitedLevel));
        }

        [HttpGet("slots/lbp2cool")]
        [HttpGet("slots/cool")]
        public async Task<IActionResult> CoolSlots([FromQuery] int page) => await LuckyDipSlots(30 * page, 30, 69);

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

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, 
                new Dictionary<string, object>
                {
                    {
                        "hint_start", pageStart + Math.Min(pageSize, 30)
                    },
                    {
                        "total", await this.database.Slots.CountAsync()
                    }
                }
                ));
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

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response,
                new Dictionary<string, object>
                {
                    {
                        "hint_start", pageStart + Math.Min(pageSize, 30)
                    },
                    {
                        "total", await this.database.Slots.CountAsync(s => s.TeamPick)
                    }
                }));
        }

        [HttpGet("slots/lbp2luckydip")]
        public async Task<IActionResult> LuckyDipSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] int seed)
        {
            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.BadRequest();

            GameVersion gameVersion = token.GameVersion;

            IEnumerable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .OrderBy(_ => EF.Functions.Random())
                .Take(Math.Min(pageSize, 30));

            string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, 
                new Dictionary<string, object>
                {
                    {
                        "hint_start", pageStart + Math.Min(pageSize, 30)
                    },
                    {
                        "total", await this.database.Slots.CountAsync()
                    }
                }));
        }

    }
}
