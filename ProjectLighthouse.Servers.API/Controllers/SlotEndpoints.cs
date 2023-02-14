#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

/// <summary>
/// A collection of endpoints relating to slots.
/// </summary>
public class SlotEndpoints : ApiEndpointController
{
    private readonly Database database;

    public SlotEndpoints(Database database)
    {
        this.database = database;
    }

    /// <summary>
    /// Gets a list of (stripped down) slots from the database.
    /// </summary>
    /// <param name="limit">How many slots you want to retrieve.</param>
    /// <param name="skip">How many slots to skip.</param>
    /// <returns>The slot</returns>
    /// <response code="200">The slot list, if successful.</response>
    [HttpGet("slots")]
    [ProducesResponseType(typeof(List<MinimalSlot>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlots([FromQuery] int limit = 20, [FromQuery] int skip = 0)
    {
        if (skip < 0) skip = 0;
        if (limit < 0) limit = 0;
        limit = Math.Min(ServerStatics.PageSize, limit);

        IEnumerable<MinimalSlot> minimalSlots = (await this.database.Slots.OrderByDescending(s => s.FirstUploaded).Skip(skip).Take(limit).ToListAsync()).Select
            (MinimalSlot.FromSlot);

        return this.Ok(minimalSlots);
    }

    /// <summary>
    /// Gets a slot (more commonly known as a level) and its information from the database.
    /// </summary>
    /// <param name="id">The ID of the slot</param>
    /// <returns>The slot</returns>
    /// <response code="200">The slot, if successful.</response>
    /// <response code="404">The slot could not be found.</response>
    [HttpGet("slot/{id:int}")]
    [ProducesResponseType(typeof(Slot), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlot(int id)
    {
        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(u => u.SlotId == id);
        if (slot == null) return this.NotFound();

        return this.Ok(slot);
    }
}