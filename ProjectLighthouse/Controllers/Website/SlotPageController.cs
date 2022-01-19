#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// I would like to apologize in advance for anyone dealing with this file. 
// Theres probably a better way to do this with delegates but I'm tired.
// TODO: Clean up this file
// - jvyden

namespace LBPUnion.ProjectLighthouse.Controllers.Website;

[ApiController]
[Route("slot/{id:int}")]
public class SlotPageController : ControllerBase
{
    private readonly Database database;

    public SlotPageController(Database database)
    {
        this.database = database;
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Slot? heartedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (heartedSlot == null) return this.NotFound();

        await this.database.HeartLevel(user, heartedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Slot? heartedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (heartedSlot == null) return this.NotFound();

        await this.database.UnheartLevel(user, heartedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> QueueLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Slot? queuedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (queuedSlot == null) return this.NotFound();

        await this.database.QueueLevel(user, queuedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("unqueue")]
    public async Task<IActionResult> UnqueueLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Slot? queuedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (queuedSlot == null) return this.NotFound();

        await this.database.UnqueueLevel(user, queuedSlot);

        return this.Redirect(callbackUrl);
    }
}