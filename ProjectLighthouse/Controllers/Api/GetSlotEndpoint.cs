#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Api;

public class GetSlotEndpoint : ApiEndpoint
{
    private readonly Database database;

    public GetSlotEndpoint(Database database)
    {
        this.database = database;
    }

    [HttpGet("slot/{id:int}")]
    public async Task<IActionResult> OnGet(int id)
    {
        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(u => u.SlotId == id);
        if (slot == null) return this.NotFound();

        return this.Ok(slot);
    }
}