#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/slots")]
[Produces("text/xml")]
public class SearchController : ControllerBase
{
    private readonly DatabaseContext database;
    public SearchController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchSlots([FromQuery] string query, string? keyName = "slots")
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        if (string.IsNullOrWhiteSpace(query)) return this.BadRequest();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        queryBuilder.AddFilter(new TextFilter(query));

        pageData.TotalElements = await this.database.Slots.Where(queryBuilder.Build()).CountAsync();

        SlotSortBuilder<SlotEntity> sortBuilder = new();
        sortBuilder.AddSort(new LastUpdatedSort());

        List<SlotBase> slots = await this.database.Slots.Include(s => s.Creator)
            .GetSlots(token, queryBuilder, pageData, sortBuilder);

        return this.Ok(new GenericSlotResponse(keyName, slots, pageData));
    }

    // /LITTLEBIGPLANETPS3_XML?pageStart=1&pageSize=10&resultTypes[]=slot&resultTypes[]=playlist&resultTypes[]=user&adventure=dontCare&textFilter=qwer
}
