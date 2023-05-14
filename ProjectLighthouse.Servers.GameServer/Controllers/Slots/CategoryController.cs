using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CategoryController : ControllerBase
{
    private readonly DatabaseContext database;

    public CategoryController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("searches")]
    [HttpGet("genres")]
    public async Task<IActionResult> GenresAndSearches()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        List<GameCategory> categories = new();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        foreach (Category category in CategoryHelper.Categories.ToList())
        {
            if(category is CategoryWithUser categoryWithUser) categories.Add(await categoryWithUser.Serialize(this.database, user, queryBuilder));
            else categories.Add(await category.Serialize(this.database, queryBuilder));
        }

        return this.Ok(new CategoryListResponse(categories, CategoryHelper.Categories.Count, 0, 1));
    }

    [HttpGet("searches/{endpointName}")]
    public async Task<IActionResult> GetCategorySlots(string endpointName)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        Category? category = CategoryHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
        if (category == null) return this.NotFound();

        PaginationData pageData = this.Request.GetPaginationData();

        Logger.Debug("Found category " + category, LogArea.Category);

        IQueryable<SlotEntity> slotQuery;

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        if (category is CategoryWithUser categoryWithUser)
        {
            int totalSlots = await categoryWithUser.GetSlots(this.database, user, queryBuilder).CountAsync();
            pageData.MaxElements = totalSlots;
            slotQuery = categoryWithUser.GetSlots(this.database, user, queryBuilder).ApplyPagination(pageData);
        }
        else
        {
            int totalSlots = await category.GetSlots(this.database, queryBuilder).CountAsync();
            pageData.MaxElements = totalSlots;
            slotQuery = category.GetSlots(this.database, queryBuilder).ApplyPagination(pageData);
        }

        if (bool.TryParse(this.Request.Query["includePlayed"], out bool includePlayed) && !includePlayed)
        {
            slotQuery = slotQuery.Select(s => new SlotMetadata
                {
                    Slot = s,
                    Played = this.database.VisitedLevels.Any(v => v.SlotId == s.SlotId && v.UserId == token.UserId),
                })
                .Where(s => !s.Played)
                .Select(s => s.Slot);
        }

        if (this.Request.Query.ContainsKey("sort"))
        {
            string sort = (string?)this.Request.Query["sort"] ?? "";
            slotQuery = sort switch
            {
                "relevance" => slotQuery.ApplyOrdering(new SlotSortBuilder<SlotEntity>()
                    .AddSort(new UniquePlaysTotalSort())
                    .AddSort(new LastUpdatedSort())),
                "likes" => slotQuery.Select(s => new SlotMetadata
                    {
                        Slot = s,
                        ThumbsUp = this.database.RatedLevels.Count(r => r.SlotId == s.SlotId && r.Rating == 1),
                    })
                    .OrderByDescending(s => s.Hearts)
                    .Select(s => s.Slot),
                "hearts" => slotQuery.Select(s => new SlotMetadata
                    {
                        Slot = s,
                        Hearts = this.database.HeartedLevels.Count(h => h.SlotId == s.SlotId),
                    })
                    .OrderByDescending(s => s.Hearts)
                    .Select(s => s.Slot),
                "date" => slotQuery.ApplyOrdering(new SlotSortBuilder<SlotEntity>().AddSort(new FirstUploadedSort())),
                "plays" => slotQuery.ApplyOrdering(
                    new SlotSortBuilder<SlotEntity>().AddSort(new UniquePlaysTotalSort())),
                _ => slotQuery,
            };
        }

        List<SlotBase> slots = (await slotQuery.ToListAsync())
            .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));

        return this.Ok(new GenericSlotResponse("results", slots, pageData));
    }

}