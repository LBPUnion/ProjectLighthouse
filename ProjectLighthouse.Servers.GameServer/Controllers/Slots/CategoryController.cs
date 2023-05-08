using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
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

        foreach (Category category in CategoryHelper.Categories.ToList())
        {
            if(category is CategoryWithUser categoryWithUser) categories.Add(categoryWithUser.Serialize(this.database, user));
            else categories.Add(category.Serialize(this.database));
        }

        return this.Ok(new CategoryListResponse(categories, CategoryHelper.Categories.Count, 0, 1));
    }

    [HttpGet("searches/{endpointName}")]
    public async Task<IActionResult> GetCategorySlots(string endpointName, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        Category? category = CategoryHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
        if (category == null) return this.NotFound();

        Logger.Debug("Found category " + category, LogArea.Category);

        List<SlotBase> slots;
        int totalSlots;

        if (category is CategoryWithUser categoryWithUser)
        {
            slots = (await categoryWithUser.GetSlots(this.database, user, pageStart, pageSize)
                .ToListAsync())
                .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));
            totalSlots = categoryWithUser.GetTotalSlots(this.database, user);
        }
        else
        {
            slots = category.GetSlots(this.database, pageStart, pageSize)
                .ToList()
                .ToSerializableList(s => SlotBase.CreateFromEntity(s, token));
            totalSlots = category.GetTotalSlots(this.database);
        }

        return this.Ok(new GenericSlotResponse("results", slots, totalSlots, pageStart + pageSize));
    }

}