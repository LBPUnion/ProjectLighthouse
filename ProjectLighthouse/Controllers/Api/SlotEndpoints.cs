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

public class SlotEndpoints : ApiEndpointController
{
    private readonly Database database;

    public SlotEndpoints(Database database)
    {
        this.database = database;
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetSlots([FromQuery] int limit = 20, [FromQuery] int skip = 0)
    {
        limit = Math.Min(ServerStatics.PageSize, limit);

        IEnumerable<MinimalSlot> minimalSlots = (await this.database.Slots.OrderByDescending(s => s.FirstUploaded).Skip(skip).Take(limit).ToListAsync()).Select
            (MinimalSlot.FromSlot);

        return this.Ok(minimalSlots);
    }

    [HttpGet("slot/{id:int}")]
    public async Task<IActionResult> GetSlot(int id)
    {
        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(u => u.SlotId == id);
        if (slot == null) return this.NotFound();

        return this.Ok(slot);
    }
}