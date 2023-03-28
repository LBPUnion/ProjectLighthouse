#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
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

    [HttpGet("searchLBP3")]
    public Task<IActionResult> SearchSlotsLBP3([FromQuery] int pageSize, [FromQuery] int pageStart, [FromQuery] string textFilter) 
        => this.SearchSlots(textFilter, pageSize, pageStart, "results");

    [HttpGet("search")]
    public async Task<IActionResult> SearchSlots(
        [FromQuery] string query,
        [FromQuery] int pageSize,
        [FromQuery] int pageStart,
        string? keyName = "slots"
    )
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        if (string.IsNullOrWhiteSpace(query)) return this.BadRequest();

        query = query.ToLower();

        string[] keywords = query.Split(" ");

        IQueryable<SlotEntity> dbQuery = this.database.Slots.ByGameVersion(token.GameVersion, false, true)
            .Where(s => s.Type == SlotType.User)
            .OrderBy(s => !s.TeamPick)
            .ThenByDescending(s => s.FirstUploaded)
            .Where(s => s.SlotId >= 0); // dumb query to conv into IQueryable

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (string keyword in keywords)
            dbQuery = dbQuery.Where
            (
                s => s.Name.ToLower().Contains(keyword) ||
                     s.Description.ToLower().Contains(keyword) ||
                     s.Creator!.Username.ToLower().Contains(keyword) ||
                     s.SlotId.ToString().Equals(keyword)
            );

        List<SlotBase> slots = await dbQuery.Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .Select(s => SlotBase.CreateFromEntity(s, token))
            .ToListAsync();

        return this.Ok(new GenericSlotResponse(keyName, slots, await dbQuery.CountAsync(), 0));
    }
    
    // /LITTLEBIGPLANETPS3_XML?pageStart=1&pageSize=10&resultTypes[]=slot&resultTypes[]=playlist&resultTypes[]=user&adventure=dontCare&textFilter=qwer

}
