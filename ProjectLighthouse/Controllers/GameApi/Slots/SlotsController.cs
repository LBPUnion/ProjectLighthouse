#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Reviews;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Slots;

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
            this.database.Slots.ByGameVersion(gameVersion, token.UserId == user.UserId, true)
                .Where(s => s.Creator!.Username == user.Username)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, ServerSettings.Instance.EntitledSlots)),
            string.Empty,
            (current, slot) => current + slot.Serialize(token.GameVersion)
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

        Slot? slot = await this.database.Slots.ByGameVersion(gameVersion, true, true).FirstOrDefaultAsync(s => s.SlotId == id);

        if (slot == null) return this.NotFound();

        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == id && r.UserId == user.UserId);
        VisitedLevel? visitedLevel = await this.database.VisitedLevels.FirstOrDefaultAsync(r => r.SlotId == id && r.UserId == user.UserId);
        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == id && r.ReviewerId == user.UserId);
        return this.Ok(slot.Serialize(gameVersion, ratedLevel, visitedLevel, review));
    }

    [HttpGet("slots/cool")]
    public async Task<IActionResult> Lbp1CoolSlots([FromQuery] int page)
    {
        const int pageSize = 30;
        return await this.CoolSlots((page - 1) * pageSize, pageSize);
    }

    [HttpGet("slots/lbp2cool")]
    public async Task<IActionResult> CoolSlots
    (
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null,
        [FromQuery] int? page = null
    )
    {
        int _pageStart = pageStart;
        if (page != null) _pageStart = (int)page * 30;
        // bit of a better placeholder until we can track average user interaction with /stream endpoint
        return await this.ThumbsSlots(_pageStart, Math.Min(pageSize, 30), gameFilterType, players, move, "thisWeek");
    }

    [HttpGet("slots")]
    public async Task<IActionResult> NewestSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        GameVersion gameVersion = token.GameVersion;

        IQueryable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true)
            .OrderByDescending(s => s.FirstUploaded)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30));

        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize(gameVersion));

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

        IQueryable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true)
            .Where(s => s.TeamPick)
            .OrderByDescending(s => s.LastUpdated)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30));
        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize(gameVersion));

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
                        "total", await StatisticsHelper.TeamPickCount()
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

        IEnumerable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true).OrderBy(_ => EF.Functions.Random()).Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(gameVersion));

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
    public async Task<IActionResult> ThumbsSlots
    (
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null,
        [FromQuery] string? dateFilterType = null
    )
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Random rand = new();

        IEnumerable<Slot> slots = this.filterByRequest(gameFilterType, dateFilterType, token.GameVersion)
            .AsEnumerable()
            .OrderByDescending(s => s.Thumbsup)
            .ThenBy(_ => rand.Next())
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));

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

    [HttpGet("slots/mostUniquePlays")]
    public async Task<IActionResult> MostUniquePlaysSlots
    (
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null,
        [FromQuery] string? dateFilterType = null
    )
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Random rand = new();

        IEnumerable<Slot> slots = this.filterByRequest(gameFilterType, dateFilterType, token.GameVersion)
            .AsEnumerable()
            .OrderByDescending
            (
                // probably not the best way to do this?
                s =>
                {
                    return this.getGameFilter(gameFilterType, token.GameVersion) switch
                    {
                        GameVersion.LittleBigPlanet1 => s.PlaysLBP1Unique,
                        GameVersion.LittleBigPlanet2 => s.PlaysLBP2Unique,
                        GameVersion.LittleBigPlanet3 => s.PlaysLBP3Unique,
                        GameVersion.LittleBigPlanetVita => s.PlaysLBPVitaUnique,
                        _ => s.PlaysUnique,
                    };
                }
            )
            .ThenBy(_ => rand.Next())
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));

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

    [HttpGet("slots/mostHearted")]
    public async Task<IActionResult> MostHeartedSlots
    (
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null,
        [FromQuery] string? dateFilterType = null
    )
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Random rand = new();

        IEnumerable<Slot> slots = this.filterByRequest(gameFilterType, dateFilterType, token.GameVersion)
            .AsEnumerable()
            .OrderByDescending(s => s.Hearts)
            .ThenBy(_ => rand.Next())
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));

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

    private GameVersion getGameFilter(string? gameFilterType, GameVersion version)
    {
        if (version == GameVersion.LittleBigPlanetVita) return GameVersion.LittleBigPlanetVita;
        if (version == GameVersion.LittleBigPlanetPSP) return GameVersion.LittleBigPlanetPSP;

        return gameFilterType switch
        {
            "lbp1" => GameVersion.LittleBigPlanet1,
            "lbp2" => GameVersion.LittleBigPlanet2,
            "lbp3" => GameVersion.LittleBigPlanet3,
            "both" => GameVersion.LittleBigPlanet2, // LBP2 default option
            null => GameVersion.LittleBigPlanet1,
            _ => GameVersion.Unknown,
        };
    }

    private IQueryable<Slot> filterByRequest(string? gameFilterType, string? dateFilterType, GameVersion version)
    {
        if (version == GameVersion.LittleBigPlanetVita || version == GameVersion.LittleBigPlanetPSP || version == GameVersion.Unknown)
        {
            return this.database.Slots.ByGameVersion(version, false, true);
        }

        string _dateFilterType = dateFilterType ?? "";

        long oldestTime = _dateFilterType switch
        {
            "thisWeek" => DateTimeOffset.Now.AddDays(-7).ToUnixTimeMilliseconds(),
            "thisMonth" => DateTimeOffset.Now.AddDays(-31).ToUnixTimeMilliseconds(),
            _ => 0,
        };

        GameVersion gameVersion = this.getGameFilter(gameFilterType, version);

        IQueryable<Slot> whereSlots;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (gameFilterType == "both")
            // Get game versions less than the current version
            // Needs support for LBP3 ("both" = LBP1+2)
            whereSlots = this.database.Slots.Where(s => s.GameVersion <= gameVersion && s.FirstUploaded >= oldestTime);
        else
            // Get game versions exactly equal to gamefiltertype
            whereSlots = this.database.Slots.Where(s => s.GameVersion == gameVersion && s.FirstUploaded >= oldestTime);

        return whereSlots.Include(s => s.Creator).Include(s => s.Location);
    }
}
