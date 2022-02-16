using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class SearchController : ControllerBase
{
    private readonly Database database;
    public SearchController(Database database)
    {
        this.database = database;
    }

    [HttpGet("slots/search")]
    public async Task<IActionResult> SearchSlots([FromQuery] string query, [FromQuery] int pageSize, [FromQuery] int pageStart)
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        if (query == null) return this.BadRequest();

        query = query.ToLower();

        string[] keywords = query.Split(" ");

        IQueryable<Slot> dbQuery = this.database.Slots
            .Include(s => s.Creator)
            .Include(s => s.Location)
            .Where(s => s.SlotId >= 0); // dumb query to conv into IQueryable

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (string keyword in keywords)
            dbQuery = dbQuery.Where
            (
                s => s.Name.ToLower().Contains(keyword) ||
                     s.Description.ToLower().Contains(keyword) ||
                     s.Creator.Username.ToLower().Contains(keyword) ||
                     s.SlotId.ToString().Equals(keyword)
            );

        List<Slot> slots = await dbQuery.Skip(pageStart - 1).Take(Math.Min(pageSize, 30)).ToListAsync();

        string response = slots.Aggregate("", (current, slot) => current + slot.Serialize(gameToken.GameVersion));

        return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", dbQuery.Count()));
    }
}