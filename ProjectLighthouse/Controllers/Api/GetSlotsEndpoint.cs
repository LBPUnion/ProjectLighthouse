using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Api;

public class GetSlotsEndpoint : ApiEndpoint
{
    private readonly Database database;

    public GetSlotsEndpoint(Database database)
    {
        this.database = database;
    }

    [HttpGet("slots")]
    public async Task<IActionResult> OnGet([FromQuery] int limit = 20, [FromQuery] int skip = 0)
    {
        limit = Math.Min(ServerStatics.PageSize, limit);

        IEnumerable<MinimalSlot> minimalSlots = (await this.database.Slots.OrderByDescending(s => s.FirstUploaded).Skip(skip).Take(limit).ToListAsync()).Select
            (MinimalSlot.FromSlot);

        return this.Ok(minimalSlots);
    }
}