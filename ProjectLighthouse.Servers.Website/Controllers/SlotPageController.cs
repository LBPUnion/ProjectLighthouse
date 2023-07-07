#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// I would like to apologize in advance for anyone dealing with this file. 
// Theres probably a better way to do this with delegates but I'm tired.
// TODO: Clean up this file
// - jvyden

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
[Route("slot/{id:int}")]
public class SlotPageController : ControllerBase
{
    private readonly DatabaseContext database;

    public SlotPageController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("unpublish")]
    public async Task<IActionResult> UnpublishSlot([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        SlotEntity? targetSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (targetSlot == null) return this.Redirect("~/slots/0");

        if (targetSlot.CreatorId != token.UserId) return this.Redirect("~/slot/" + id);

        this.database.Slots.Remove(targetSlot);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/slots/0");
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        SlotEntity? heartedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (heartedSlot == null) return this.NotFound();

        await this.database.HeartLevel(token.UserId, heartedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        SlotEntity? heartedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (heartedSlot == null) return this.NotFound();

        await this.database.UnheartLevel(token.UserId, heartedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> QueueLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        SlotEntity? queuedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (queuedSlot == null) return this.NotFound();

        await this.database.QueueLevel(token.UserId, queuedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("unqueue")]
    public async Task<IActionResult> UnqueueLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        SlotEntity? queuedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (queuedSlot == null) return this.NotFound();

        await this.database.UnqueueLevel(token.UserId, queuedSlot);

        return this.Redirect(callbackUrl);
    }
}