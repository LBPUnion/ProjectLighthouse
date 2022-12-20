#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Match.Rooms;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Reviews;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class SlotsController : ControllerBase
{
    private readonly Database database;
    public SlotsController(Database database)
    {
        this.database = database;
    }

    private static string generateSlotsResponse(string slotAggregate, int start, int total) =>
        LbpSerializer.TaggedStringElement("slots",
            slotAggregate,
            new Dictionary<string, object>
            {
                {
                    "hint_start", start
                },
                {
                    "total", total
                },
            });

    [HttpGet("slots/by")]
    public async Task<IActionResult> SlotsBy([FromQuery(Name="u")] string username, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        int targetUserId = await this.database.UserIdFromUsername(username);
        if (targetUserId == 0) return this.NotFound();

        int usedSlots = this.database.Slots.Count(s => s.CreatorId == targetUserId);

        string response = Enumerable.Aggregate
        (
            this.database.Slots.Where(s => s.CreatorId == targetUserId)
                .ByGameVersion(gameVersion, token.UserId == targetUserId, true)
                .Skip(Math.Max(0, pageStart - 1))
                .Take(Math.Min(pageSize, usedSlots)),
            string.Empty,
            (current, slot) => current + slot.Serialize(token.GameVersion)
        );
        int start = pageStart + Math.Min(pageSize, usedSlots);
        int total = await this.database.Slots.CountAsync(s => s.CreatorId == targetUserId);
        return this.Ok(generateSlotsResponse(response, start, total));
    }

    [HttpGet("slotList")]
    public async Task<IActionResult> GetSlotListAlt([FromQuery] int[] s)
    {
        List<string?> serializedSlots = new();
        foreach (int slotId in s)
        {
            Slot? slot = await this.database.Slots.Include(t => t.Creator).Include(t => t.Location).Where(t => t.SlotId == slotId && t.Type == SlotType.User).FirstOrDefaultAsync();
            if (slot == null)
            {
                slot = await this.database.Slots.Where(t => t.InternalSlotId == slotId && t.Type == SlotType.Developer).FirstOrDefaultAsync();
                if (slot == null)
                {
                    serializedSlots.Add($"<slot type=\"developer\"><id>{slotId}</id></slot>");
                    continue;
                }
            }
            serializedSlots.Add(slot.Serialize());
        }
        string serialized = serializedSlots.Aggregate(string.Empty, (current, slot) => slot == null ? current : current + slot);

        return this.Ok(LbpSerializer.TaggedStringElement("slots", serialized, "total", serializedSlots.Count));
    }

    [HttpGet("slots/developer")]
    public async Task<IActionResult> StoryPlayers()
    {
        List<int> activeSlotIds = RoomHelper.Rooms.Where(r => r.Slot.SlotType == SlotType.Developer).Select(r => r.Slot.SlotId).ToList();

        List<string> serializedSlots = new();

        foreach (int id in activeSlotIds)
        {
            int placeholderSlotId = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);
            Slot slot = await this.database.Slots.FirstAsync(s => s.SlotId == placeholderSlotId);
            serializedSlots.Add(slot.SerializeDevSlot());
        }

        string serialized = serializedSlots.Aggregate(string.Empty, (current, slot) => current + slot);

        return this.Ok(LbpSerializer.StringElement("slots", serialized));
    }

    [HttpGet("s/developer/{id:int}")]
    public async Task<IActionResult> SDev(int id)
    {
        int slotId = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);
        Slot slot = await this.database.Slots.FirstAsync(s => s.SlotId == slotId);

        return this.Ok(slot.SerializeDevSlot());
    } 

    [HttpGet("s/user/{id:int}")]
    public async Task<IActionResult> SUser(int id)
    {
        GameToken token = this.GetToken();

        GameVersion gameVersion = token.GameVersion;

        Slot? slot = await this.database.Slots.ByGameVersion(gameVersion, true, true).FirstOrDefaultAsync(s => s.SlotId == id);

        if (slot == null) return this.NotFound();

        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == id && r.UserId == token.UserId);
        VisitedLevel? visitedLevel = await this.database.VisitedLevels.FirstOrDefaultAsync(r => r.SlotId == id && r.UserId == token.UserId);
        Review? review = await this.database.Reviews.Include(r => r.Slot).FirstOrDefaultAsync(r => r.SlotId == id && r.ReviewerId == token.UserId);
        return this.Ok(slot.Serialize(gameVersion, ratedLevel, visitedLevel, review, true));
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
        if (page != null) pageStart = (int)page * 30;
        // bit of a better placeholder until we can track average user interaction with /stream endpoint
        return await this.ThumbsSlots(pageStart, Math.Min(pageSize, 30), gameFilterType, players, move, "thisWeek");
    }

    [HttpGet("slots")]
    public async Task<IActionResult> NewestSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        IQueryable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true)
            .OrderByDescending(s => s.FirstUploaded)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize(gameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.SlotCountForGame(this.database, token.GameVersion);
        return this.Ok(generateSlotsResponse(response, start, total));
    }

    [HttpGet("slots/like/{slotType}/{slotId:int}")]
    public async Task<IActionResult> SimilarSlots([FromRoute] string slotType, [FromRoute] int slotId, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        if (slotType != "user") return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        Slot? targetSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (targetSlot == null) return this.BadRequest();

        string[] tags = targetSlot.LevelTags;

        List<int> slotIdsWithTag = this.database.RatedLevels
            .Where(r => r.TagLBP1.Length > 0)
            .Where(r => tags.Contains(r.TagLBP1))
            .Select(r => r.SlotId)
            .ToList();

        IQueryable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true)
            .Where(s => slotIdsWithTag.Contains(s.SlotId))
            .OrderByDescending(s => s.PlaysLBP1)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize(gameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = slotIdsWithTag.Count;

        return this.Ok(generateSlotsResponse(response, start, total));
    }

    [HttpGet("slots/highestRated")]
    public async Task<IActionResult> HighestRatedSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true)
            .AsEnumerable()
            .OrderByDescending(s => s.RatingLBP1)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(gameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.SlotCount(this.database); 

        return this.Ok(generateSlotsResponse(response, start, total));
    }

    [HttpGet("slots/tag")]
    public async Task<IActionResult> SimilarSlots([FromQuery] string tag, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        List<int> slotIdsWithTag = await this.database.RatedLevels.Where(r => r.TagLBP1.Length > 0)
            .Where(r => r.TagLBP1 == tag)
            .Select(s => s.SlotId)
            .ToListAsync();

        IQueryable<Slot> slots = this.database.Slots.Where(s => slotIdsWithTag.Contains(s.SlotId))
            .ByGameVersion(gameVersion, false, true)
            .OrderByDescending(s => s.PlaysLBP1)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize(gameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = slotIdsWithTag.Count;

        return this.Ok(generateSlotsResponse(response, start, total));
    }

    [HttpGet("slots/mmpicks")]
    public async Task<IActionResult> TeamPickedSlots([FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        IQueryable<Slot> slots = this.database.Slots.Where(s => s.TeamPick)
            .ByGameVersion(gameVersion, false, true)
            .OrderByDescending(s => s.LastUpdated)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));
        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize(gameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.TeamPickCount(this.database);

        return this.Ok(generateSlotsResponse(response, start, total));
    }

    [HttpGet("slots/lbp2luckydip")]
    public async Task<IActionResult> LuckyDipSlots([FromQuery] int pageStart, [FromQuery] int pageSize, [FromQuery] int seed)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Slot> slots = this.database.Slots.ByGameVersion(gameVersion, false, true).OrderBy(_ => EF.Functions.Random()).Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(gameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.SlotCountForGame(this.database, token.GameVersion);

        return this.Ok(generateSlotsResponse(response, start, total));
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
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        Random rand = new();

        IEnumerable<Slot> slots = this.filterByRequest(gameFilterType, dateFilterType, token.GameVersion)
            .AsEnumerable()
            .OrderByDescending(s => s.Thumbsup)
            .ThenBy(_ => rand.Next())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.SlotCountForGame(this.database, token.GameVersion);

        return this.Ok(generateSlotsResponse(response, start, total));
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
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

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
                        GameVersion.LittleBigPlanetVita => s.PlaysLBP2Unique,
                        _ => s.PlaysUnique,
                    };
                }
            )
            .ThenBy(_ => rand.Next())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.SlotCountForGame(this.database, token.GameVersion);

        return this.Ok(generateSlotsResponse(response, start, total));
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
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        Random rand = new();

        IEnumerable<Slot> slots = this.filterByRequest(gameFilterType, dateFilterType, token.GameVersion)
            .AsEnumerable()
            .OrderByDescending(s => s.Hearts)
            .ThenBy(_ => rand.Next())
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30));

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = await StatisticsHelper.SlotCountForGame(this.database, token.GameVersion);

        return this.Ok(generateSlotsResponse(response, start, total));
    }
    
    // /slots/busiest?pageStart=1&pageSize=30&gameFilterType=both&players=1&move=true
    [HttpGet("slots/busiest")]
    public async Task<IActionResult> BusiestLevels
    (
        [FromQuery] int pageStart,
        [FromQuery] int pageSize,
        [FromQuery] string? gameFilterType = null,
        [FromQuery] int? players = null,
        [FromQuery] bool? move = null
    )
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        Dictionary<int, int> playersBySlotId = new();

        foreach (Room room in RoomHelper.Rooms)
        {
            // TODO: support developer slotTypes?
            if (room.Slot.SlotType != SlotType.User) continue;

            if (!playersBySlotId.TryGetValue(room.Slot.SlotId, out int playerCount)) 
                playersBySlotId.Add(room.Slot.SlotId, 0);

            playerCount += room.PlayerIds.Count;

            playersBySlotId.Remove(room.Slot.SlotId);
            playersBySlotId.Add(room.Slot.SlotId, playerCount);
        }

        IEnumerable<int> orderedPlayersBySlotId = playersBySlotId
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key);
        
        List<Slot> slots = new();

        foreach (int slotId in orderedPlayersBySlotId)
        {
            Slot? slot = await this.database.Slots.ByGameVersion(token.GameVersion, false, true)
                .FirstOrDefaultAsync(s => s.SlotId == slotId);
            if (slot == null) continue; // shouldn't happen ever unless the room is borked
            
            slots.Add(slot);
        }

        string response = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(token.GameVersion));
        int start = pageStart + Math.Min(pageSize, ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots);
        int total = playersBySlotId.Count;

        return this.Ok(generateSlotsResponse(response, start, total));
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
            whereSlots = this.database.Slots.Where(s => s.Type == SlotType.User && !s.Hidden && s.GameVersion <= gameVersion && s.FirstUploaded >= oldestTime);
        else
            // Get game versions exactly equal to gamefiltertype
            whereSlots = this.database.Slots.Where(s => s.Type == SlotType.User && !s.Hidden && s.GameVersion == gameVersion && s.FirstUploaded >= oldestTime);

        return whereSlots.Include(s => s.Creator).Include(s => s.Location);
    }
}