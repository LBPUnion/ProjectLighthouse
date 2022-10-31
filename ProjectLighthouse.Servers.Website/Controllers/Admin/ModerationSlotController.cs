#nullable enable
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Helpers;
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

        await this.database.SaveChangesAsync();
        await this.database.CreateActivity(slot.SlotId, ActivityType.TeamPick, TimeHelper.UnixTimeMilliseconds());

        return this.Redirect("~/slot/" + id);
    }

    [HttpGet("removeTeamPick")]
    public async Task<IActionResult> RemoveTeamPick([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();
        slot.TeamPick = false;

        await this.database.SaveChangesAsync();
        await this.database.DeleteActivity(slot.SlotId, ActivityType.TeamPick);

        return this.Redirect("~/slot/" + id);
    }

    [HttpGet("delete")]
    public async Task<IActionResult> DeleteLevel([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.Ok();

        await this.database.RemoveSlot(slot);

        return this.Redirect("~/slots/0");
    }
}