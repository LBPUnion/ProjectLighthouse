#nullable enable
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Admin;

[ApiController]
[Route("moderation/slot/{id:int}")]
public class ModerationSlotController : ControllerBase
{
    private readonly Database database;

    public ModerationSlotController(Database database)
    {
        this.database = database;
    }

    [HttpGet("teamPick")]
    public async Task<IActionResult> TeamPick([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();
        slot.TeamPick = true;
        this.database.Cases.Add(ModerationCase.NewTeamPickCase(user.UserId, slot.SlotId, true));

        await this.database.SaveChangesAsync();
        return this.Ok();
    }

    [HttpGet("removeTeamPick")]
    public async Task<IActionResult> RemoveTeamPick([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();
        slot.TeamPick = false;
        this.database.Cases.Add(ModerationCase.NewTeamPickCase(user.UserId, slot.SlotId, false));

        await this.database.SaveChangesAsync();
        return this.Ok();
    }

    [HttpGet("delete")]
    public async Task<IActionResult> DeleteLevel([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.Ok();

        await this.database.RemoveSlot(slot);

        return this.Ok();
    }
}