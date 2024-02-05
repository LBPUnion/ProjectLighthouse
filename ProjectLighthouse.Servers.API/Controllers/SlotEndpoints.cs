#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
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
    private readonly DatabaseContext database;

    public SlotEndpoints(DatabaseContext database)
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
    [ProducesResponseType(typeof(List<ApiSlot>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlots([FromQuery] int limit = 20, [FromQuery] int skip = 0)
    {
        if (skip < 0) skip = 0;
        if (limit < 0) limit = 0;
        limit = Math.Min(ServerStatics.PageSize, limit);

        List<ApiSlot> minimalSlots = (await this.database.Slots.OrderByDescending(s => s.FirstUploaded)
            .Skip(skip)
            .Take(limit)
            .ToListAsync()).ToSerializableList(MinimalApiSlot.CreateFromEntity);

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
    [ProducesResponseType(typeof(ApiSlot), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlot(int id)
    {
        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(u => u.SlotId == id);
        if (slot == null) return this.NotFound();

        return this.Ok(ApiSlot.CreateFromEntity(slot, this.database));
    }
}