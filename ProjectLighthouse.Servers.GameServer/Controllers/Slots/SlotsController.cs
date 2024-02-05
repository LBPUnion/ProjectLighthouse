#nullable enable
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Filter.Sorts.Metadata;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
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
    private readonly DatabaseContext database;

    public SlotsController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("slots/by")]
    public async Task<IActionResult> SlotsBy([FromQuery(Name = "u")] string username)
    {
        GameTokenEntity token = this.GetToken();

        int targetUserId = await this.database.UserIdFromUsername(username);
        if (targetUserId == 0) return this.NotFound();

        PaginationData pageData = this.Request.GetPaginationData();

        pageData.TotalElements = await this.database.Slots.CountAsync(s => s.CreatorId == targetUserId);

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token).AddFilter(new CreatorFilter(targetUserId));

        SlotSortBuilder<SlotEntity> sortBuilder = new SlotSortBuilder<SlotEntity>()
            .AddSort(new FirstUploadedSort())
            .SortDescending(false);

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse("slots", slots, pageData));
    }

    [HttpGet("slotList")]
    public async Task<IActionResult> GetSlotListAlt([FromQuery(Name = "s")] int[] slotIds)
    {
        GameTokenEntity token = this.GetToken();

        List<SlotBase> slots = new();
        foreach (int slotId in slotIds)
        {
            SlotEntity? slot = await this.database.Slots.Where(t => t.SlotId == slotId && t.Type == SlotType.User).FirstOrDefaultAsync();
            if (slot == null)
            {
                slot = await this.database.Slots.Where(t => t.InternalSlotId == slotId && t.Type == SlotType.Developer).FirstOrDefaultAsync();
                if (slot == null)
                {
                    slots.Add(new GameDeveloperSlot
                    {
                        InternalSlotId = slotId,
                    });
                    continue;
                }
            }
            
            slots.Add(SlotBase.CreateFromEntity(slot, token));
        }

        return this.Ok(new GenericSlotResponse(slots, slots.Count, 0));
    }

    [HttpGet("slots/developer")]
    public async Task<IActionResult> StoryPlayers()
    {
        GameTokenEntity token = this.GetToken();

        List<int> activeSlotIds = RoomHelper.Rooms.Where(r => r.Slot.SlotType == SlotType.Developer).Select(r => r.Slot.SlotId).ToList();

        List<SlotBase> slots = new();

        foreach (int id in activeSlotIds)
        {
            int placeholderSlotId = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);
            SlotEntity slot = await this.database.Slots.FirstAsync(s => s.SlotId == placeholderSlotId);

            slots.Add(SlotBase.CreateFromEntity(slot, token));
        }

        return this.Ok(new GenericSlotResponse(slots));
    }

    [HttpGet("s/developer/{id:int}")]
    public async Task<IActionResult> DeveloperSlot(int id)
    {
        GameTokenEntity token = this.GetToken();

        int slotId = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);
        SlotEntity slot = await this.database.Slots.FirstAsync(s => s.SlotId == slotId);

        return this.Ok(SlotBase.CreateFromEntity(slot, token));
    } 

    [HttpGet("s/user/{id:int}")]
    public async Task<IActionResult> UserSlot(int id)
    {
        GameTokenEntity token = this.GetToken();

        SlotEntity? slot = await this.database.Slots.Where(this.GetDefaultFilters(token).Build())
            .FirstOrDefaultAsync(s => s.SlotId == id);

        if (slot == null) return this.NotFound();

        return this.Ok(SlotBase.CreateFromEntity(slot, token, SerializationMode.Full));
    }

    [HttpGet("slots/cool")]
    public async Task<IActionResult> Lbp1CoolSlots() => await this.CoolSlots();

    [HttpGet("slots/lbp2cool")]
    public async Task<IActionResult> CoolSlots() => await this.ThumbsSlots();

    [HttpGet("slots")]
    public async Task<IActionResult> NewestSlots()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new FirstUploadedSort());
        sortBuilder.AddSort(new SlotIdSort());

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/like/{slotType}/{slotId:int}")]
    public async Task<IActionResult> SimilarSlots([FromRoute] string slotType, [FromRoute] int slotId)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        if (slotType != "user") return this.BadRequest();

        SlotEntity? targetSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (targetSlot == null) return this.BadRequest();

        string[] tags = targetSlot.LevelTags(this.database);

        List<int> slotIdsWithTag = this.database.RatedLevels
            .Where(r => r.TagLBP1.Length > 0)
            .Where(r => tags.Contains(r.TagLBP1))
            .Select(r => r.SlotId)
            .ToList();

        pageData.TotalElements = slotIdsWithTag.Count;

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token).AddFilter(0, new SlotIdFilter(slotIdsWithTag));

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new PlaysForGameSort(GameVersion.LittleBigPlanet1));

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/highestRated")]
    public async Task<IActionResult> HighestRatedSlots()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotMetadata> sortBuilder = new();
        sortBuilder.AddSort(new RatingLBP1Sort());

        Expression<Func<SlotEntity, SlotMetadata>> selectorFunc = s => new SlotMetadata
        {
            Slot = s,
            RatingLbp1 = this.database.RatedLevels.Where(r => r.SlotId == s.SlotId)
                .Average(r => (double?)r.RatingLBP1) ?? 3.0,
        };

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder, selectorFunc);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/tag")]
    public async Task<IActionResult> SimilarSlots([FromQuery] string tag)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        List<int> slotIdsWithTag = await this.database.RatedLevels.Where(r => r.TagLBP1.Length > 0)
            .Where(r => r.TagLBP1 == tag)
            .Select(s => s.SlotId)
            .ToListAsync();

        pageData.TotalElements = slotIdsWithTag.Count;

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new PlaysForGameSort(GameVersion.LittleBigPlanet1));

        List<SlotBase> slots = await this.database.GetSlots(token, this.FilterFromRequest(token), pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/mmpicks")]
    public async Task<IActionResult> TeamPickedSlots()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token).AddFilter(new TeamPickFilter());

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new LastUpdatedSort());

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/lbp2luckydip")]
    public async Task<IActionResult> LuckyDipSlots([FromQuery] int seed)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new RandomFirstUploadedSort());

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/thumbs")]
    public async Task<IActionResult> ThumbsSlots()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotMetadata> sortBuilder = new();
        sortBuilder.AddSort(new ThumbsUpSort());

        Expression<Func<SlotEntity, SlotMetadata>> selectorFunc = s => new SlotMetadata
        {
            Slot = s,
            ThumbsUp = this.database.RatedLevels.Count(r => r.SlotId == s.SlotId && r.Rating == 1),
        };

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder, selectorFunc);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/mostUniquePlays")]
    public async Task<IActionResult> MostUniquePlaysSlots()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new UniquePlaysForGameSort(token.GameVersion));

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }

    [HttpGet("slots/mostHearted")]
    public async Task<IActionResult> MostHeartedSlots()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        pageData.TotalElements = await StatisticsHelper.SlotCount(this.database, queryBuilder);

        SlotSortBuilder<SlotMetadata> sortBuilder = new();
        sortBuilder.AddSort(new HeartsSort());

        Expression<Func<SlotEntity, SlotMetadata>> selectorFunc = s => new SlotMetadata
        {
            Slot = s,
            Hearts = this.database.HeartedLevels.Count(r => r.SlotId == s.SlotId),
        };

        List<SlotBase> slots = await this.database.GetSlots(token, queryBuilder, pageData, sortBuilder, selectorFunc);

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }
    
    // /slots/busiest?pageStart=1&pageSize=30&gameFilterType=both&players=1&move=true
    [HttpGet("slots/busiest")]
    public async Task<IActionResult> BusiestLevels()
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        List<int> busiestSlots = RoomHelper.Rooms.Where(r => r.Slot.SlotType == SlotType.User)
            .GroupBy(r => r.Slot.SlotId)
            .OrderByDescending(kvp => kvp.Count())
            .Select(kvp => kvp.Key)
            .AsQueryable()
            .ApplyPagination(pageData)
            .ToList();

        pageData.TotalElements = busiestSlots.Count;

        List<SlotBase> slots = new();

        Expression<Func<SlotEntity, bool>> filterQuery = this.FilterFromRequest(token).Build();

        foreach (int slotId in busiestSlots)
        {
            SlotBase? slot = await this.database.Slots.Where(s => s.SlotId == slotId)
                .Where(filterQuery)
                .Select(s => SlotBase.CreateFromEntity(s, token))
                .FirstOrDefaultAsync();
            if (slot == null) continue;
            slots.Add(slot);
        }

        return this.Ok(new GenericSlotResponse(slots, pageData));
    }
}