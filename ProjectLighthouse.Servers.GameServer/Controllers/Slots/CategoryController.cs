using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
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

        List<SlotBase> slots;

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        if (category is CategoryWithUser categoryWithUser)
        {
            
            int totalSlots = await categoryWithUser.GetSlots(this.database, user, queryBuilder).CountAsync();
            pageData.MaxElements = totalSlots;
            slots = (await categoryWithUser.GetSlots(this.database, user, queryBuilder)
                .ApplyPagination(pageData)
                .ToListAsync())
                .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));
        }
        else
        {
            int totalSlots = await category.GetSlots(this.database, queryBuilder).CountAsync();
            pageData.MaxElements = totalSlots;
            slots = (await category.GetSlots(this.database, queryBuilder)
                .ApplyPagination(pageData)
                .ToListAsync())
                .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));
        }

        return this.Ok(new GenericSlotResponse("results", slots, pageData));
    }

}