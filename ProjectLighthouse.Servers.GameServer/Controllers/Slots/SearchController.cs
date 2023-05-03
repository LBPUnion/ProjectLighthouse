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
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
    public Task<IActionResult> SearchSlotsLBP3([FromQuery] int pageSize, [FromQuery] int pageStart, [FromQuery] string textFilter,
        [FromQuery] int? players = 0,
        [FromQuery] string? labelFilter0 = null,
        [FromQuery] string? labelFilter1 = null,
        [FromQuery] string? labelFilter2 = null,
        [FromQuery] string? labelFilter3 = null,
        [FromQuery] string? labelFilter4 = null,
        [FromQuery] string? move = null,
        [FromQuery] string? adventure = null) 
        => this.SearchSlots(textFilter, pageSize, pageStart, "results", false, players+1, labelFilter0, labelFilter1, labelFilter2, labelFilter3, labelFilter4, move, adventure);

    [HttpGet("search")]
    public async Task<IActionResult> SearchSlots(
        [FromQuery] string query,
        [FromQuery] int pageSize,
        [FromQuery] int pageStart,
        string? keyName = "slots",
        bool crosscontrol = false,
        [FromQuery] int? players = null,
        [FromQuery] string? labelFilter0 = null,
        [FromQuery] string? labelFilter1 = null,
        [FromQuery] string? labelFilter2 = null,
        [FromQuery] string? labelFilter3 = null,
        [FromQuery] string? labelFilter4 = null,
        [FromQuery] string? move = null,
        [FromQuery] string? adventure = null
    )
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        if (string.IsNullOrWhiteSpace(query)) return this.BadRequest();

        query = query.ToLower();

        string[] keywords = query.Split(" ");

        IQueryable<SlotEntity> dbQuery = this.database.Slots.ByGameVersion(token.GameVersion, false, true)
            .Where(s => s.Type == SlotType.User && s.CrossControllerRequired == crosscontrol)
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

        List<SlotEntity> slots = (await dbQuery.Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync());

        slots = filterSlots(slots, players, labelFilter0, labelFilter1, labelFilter2, labelFilter3, labelFilter4, move, adventure);

        return this.Ok(new GenericSlotResponse(keyName, slots.ToSerializableList(s => SlotBase.CreateFromEntity(s, token)), await dbQuery.CountAsync(), 0));
    }

    // /LITTLEBIGPLANETPS3_XML?pageStart=1&pageSize=10&resultTypes[]=slot&resultTypes[]=playlist&resultTypes[]=user&adventure=dontCare&textFilter=qwer

    private List<SlotEntity> filterSlots(List<SlotEntity> slots, int? players = null, string? labelFilter0 = null, string? labelFilter1 = null, string? labelFilter2 = null, string? labelFilter3 = null, string? labelFilter4 = null, string? move = null, string? adventure = null)
    {
        if (players != null)
            slots.RemoveAll(s => s.MinimumPlayers != players);

        if (labelFilter0 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter0));
        if (labelFilter1 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter1));
        if (labelFilter2 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter2));
        if (labelFilter3 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter3));
        if (labelFilter4 != null)
            slots.RemoveAll(s => !s.AuthorLabels.Split(',').ToList().Contains(labelFilter4));

        if (move == "false")
            slots.RemoveAll(s => s.MoveRequired);
        if (move == "only")
            slots.RemoveAll(s => !s.MoveRequired);

        if (move == "noneCan")
            slots.RemoveAll(s => s.MoveRequired);
        if (move == "allMust")
            slots.RemoveAll(s => !s.MoveRequired);

        if (adventure == "noneCan")
            slots.RemoveAll(s => s.IsAdventurePlanet);
        if (adventure == "allMust")
            slots.RemoveAll(s => !s.IsAdventurePlanet);

        return slots;
    }
}
