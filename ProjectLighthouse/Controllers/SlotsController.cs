#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
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
            GameToken? token = await this.database.GameTokenFromRequest(this.Request);
            if (token == null) return this.StatusCode(403, "");

            GameVersion gameVersion = token.GameVersion;

            User? user = await this.database.Users.FirstOrDefaultAsync(dbUser => dbUser.Username == u);
            if (user == null) return this.NotFound();

            string response = Enumerable.Aggregate
            (
                this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                    .Include(s => s.Creator)
                    .Include(s => s.Location)
                    .Where(s => s.Creator!.Username == user.Username)
                    .Skip(pageStart - 1)
                    .Take(Math.Min(pageSize, ServerSettings.Instance.EntitledSlots)),
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
                            "hint_start", pageStart + Math.Min(pageSize, ServerSettings.Instance.EntitledSlots)
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
            User? user = await this.database.UserFromGameRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            GameToken? token = await this.database.GameTokenFromRequest(this.Request);
            if (token == null) return this.StatusCode(403, "");

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
        public async Task<IActionResult> CoolSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] string gameFilterType, [FromQuery] int players, [FromQuery] Boolean move, [FromQuery] int? page = null)
        {
            int _pageStart = pageStart;
            if (page != null) _pageStart = (int)page * 30;
            // bit of a better placeholder until we can track average user interaction with /stream endpoint
            return await ThumbsSlots(_pageStart, Math.Min(pageSize, 30), gameFilterType, players, move, "thisWeek");
        }

        [HttpGet("slots")]
        public async Task<IActionResult> NewestSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            GameToken? token = await this.database.GameTokenFromRequest(this.Request);
            if (token == null) return this.StatusCode(403, "");

            GameVersion gameVersion = token.GameVersion;

            IQueryable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .OrderByDescending(s => s.FirstUploaded)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));
            string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok
            (
                LbpSerializer.TaggedStringElement
                (
                    "slots",
                    response,
                    new Dictionary<string, object>
                    {
                        {
                            "hint_start", pageStart + Math.Min(pageSize, ServerSettings.Instance.EntitledSlots)
                        },
                        {
                            "total", await StatisticsHelper.SlotCount()
                        },
                    }
                )
            );
        }

        [HttpGet("slots/mmpicks")]
        public async Task<IActionResult> TeamPickedSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            GameToken? token = await this.database.GameTokenFromRequest(this.Request);
            if (token == null) return this.StatusCode(403, "");

            GameVersion gameVersion = token.GameVersion;

            IQueryable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Where(s => s.TeamPick)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .OrderByDescending(s => s.LastUpdated)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));
            string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok
            (
                LbpSerializer.TaggedStringElement
                (
                    "slots",
                    response,
                    new Dictionary<string, object>
                    {
                        {
                            "hint_start", pageStart + Math.Min(pageSize, ServerSettings.Instance.EntitledSlots)
                        },
                        {
                            "total", await StatisticsHelper.MMPicksCount()
                        },
                    }
                )
            );
        }

        [HttpGet("slots/lbp2luckydip")]
        public async Task<IActionResult> LuckyDipSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] int seed)
        {
            GameToken? token = await this.database.GameTokenFromRequest(this.Request);
            if (token == null) return this.StatusCode(403, "");

            GameVersion gameVersion = token.GameVersion;

            IEnumerable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .OrderBy(_ => EF.Functions.Random())
                .Take(Math.Min(pageSize, 30));

            string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok
            (
                LbpSerializer.TaggedStringElement
                (
                    "slots",
                    response,
                    new Dictionary<string, object>
                    {
                        {
                            "hint_start", pageStart + Math.Min(pageSize, ServerSettings.Instance.EntitledSlots)
                        },
                        {
                            "total", await StatisticsHelper.SlotCount()
                        },
                    }
                )
            );
        }

        [HttpGet("slots/thumbs")]
        public async Task<IActionResult> ThumbsSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] string gameFilterType, [FromQuery] int players, [FromQuery] Boolean move, [FromQuery] string? dateFilterType = null)
        {
            // v--- not sure of API in LBP3 here, needs testing
            GameVersion gameVersion = gameFilterType == "both" ? GameVersion.LittleBigPlanet2 : GameVersion.LittleBigPlanet1;

            long oldestTime;

            string _dateFilterType = dateFilterType != null ? dateFilterType : "";

            switch (_dateFilterType)
            {
                case "thisWeek":
                    oldestTime = DateTimeOffset.Now.AddDays(-7).ToUnixTimeMilliseconds();
                    break;
                case "thisMonth":
                    oldestTime = DateTimeOffset.Now.AddDays(-31).ToUnixTimeMilliseconds();
                    break;
                default:
                    oldestTime = 0;
                    break;
            }

            Random rand = new();

            IEnumerable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion && s.FirstUploaded >= oldestTime)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .AsEnumerable()
                .OrderByDescending(s => s.Thumbsup)
                .ThenBy(_ => rand.Next())
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));

            string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }

        [HttpGet("slots/mostUniquePlays")]
        public async Task<IActionResult> MostUniquePlaysSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] string gameFilterType, [FromQuery] int players, [FromQuery] Boolean move, [FromQuery] string? dateFilterType = null)
        {
            // v--- not sure of API in LBP3 here, needs testing
            GameVersion gameVersion = gameFilterType == "both" ? GameVersion.LittleBigPlanet2 : GameVersion.LittleBigPlanet1;

            long oldestTime;

            string _dateFilterType = dateFilterType != null ? dateFilterType : "";

            switch (_dateFilterType)
            {
                case "thisWeek":
                    oldestTime = DateTimeOffset.Now.AddDays(-7).ToUnixTimeMilliseconds();
                    break;
                case "thisMonth":
                    oldestTime = DateTimeOffset.Now.AddDays(-31).ToUnixTimeMilliseconds();
                    break;
                default:
                    oldestTime = 0;
                    break;
            }

            Random rand = new();

            IEnumerable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion && s.FirstUploaded >= oldestTime)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .AsEnumerable()
                .OrderByDescending(s =>
                {
                    // probably not the best way to do this
                    switch (gameVersion)
                    {
                        case GameVersion.LittleBigPlanet1:
                            return s.PlaysLBP1Unique;
                        case GameVersion.LittleBigPlanet2:
                            return s.PlaysLBP2Unique;
                        case GameVersion.LittleBigPlanet3:
                            return s.PlaysLBP3Unique;
                        case GameVersion.LittleBigPlanetVita:
                            return s.PlaysLBPVitaUnique;
                        default:
                            return s.PlaysUnique;
                    }
                })
                .ThenBy(_ => rand.Next())
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));

            string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }

        [HttpGet("slots/mostHearted")]
        public async Task<IActionResult> MostHeartedSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] string gameFilterType, [FromQuery] int players, [FromQuery] Boolean move, [FromQuery] string? dateFilterType = null)
        {
            // v--- not sure of API in LBP3 here, needs testing
            GameVersion gameVersion = gameFilterType == "both" ? GameVersion.LittleBigPlanet2 : GameVersion.LittleBigPlanet1;

            long oldestTime;

            string _dateFilterType = dateFilterType != null ? dateFilterType : "";

            switch (_dateFilterType)
            {
                case "thisWeek":
                    oldestTime = DateTimeOffset.Now.AddDays(-7).ToUnixTimeMilliseconds();
                    break;
                case "thisMonth":
                    oldestTime = DateTimeOffset.Now.AddDays(-31).ToUnixTimeMilliseconds();
                    break;
                default:
                    oldestTime = 0;
                    break;
            }

            Random rand = new();

            IEnumerable<Slot> slots = this.database.Slots.Where(s => s.GameVersion <= gameVersion && s.FirstUploaded >= oldestTime)
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .AsEnumerable()
                .OrderByDescending(s => s.Hearts)
                .ThenBy(_ => rand.Next())
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));

            string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "hint_start", pageStart + Math.Min(pageSize, 30)));
        }

    }
}