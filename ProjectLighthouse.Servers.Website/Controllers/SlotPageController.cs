#nullable enable
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Types;
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
    private readonly Database database;

    public SlotPageController(Database database)
    {
        this.database = database;
    }

    [HttpGet("unpublish")]
    public async Task<IActionResult> UnpublishSlot([FromRoute] int id)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        Slot? targetSlot = await this.database.Slots.Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == id);
        if (targetSlot == null) return this.Redirect("~/slots/0");

        if (targetSlot.Location == null) throw new ArgumentNullException();

        if (targetSlot.CreatorId != token.UserId) return this.Redirect("~/slot/" + id);

        this.database.Locations.Remove(targetSlot.Location);
        this.database.Slots.Remove(targetSlot);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/slots/0");
    }

    [HttpGet("rateComment")]
    public async Task<IActionResult> RateComment([FromRoute] int id, [FromQuery] int commentId, [FromQuery] int rating)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        await this.database.RateComment(token.UserId, commentId, rating);

        return this.Redirect($"~/slot/{id}#{commentId}");
    }

    [HttpPost("postComment")]
    public async Task<IActionResult> PostComment([FromRoute] int id, [FromForm] string? msg)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        if (msg == null)
        {
            Logger.Error($"Refusing to post comment from {token.UserId} on user {id}, {nameof(msg)} is null", LogArea.Comments);
            return this.Redirect("~/slot/" + id);
        }

        // Prevent potential xml injection and censor content 
        msg = SanitizationHelper.SanitizeString(msg);
        msg = CensorHelper.FilterMessage(msg);

        await this.database.PostComment(token.UserId, id, CommentType.Level, msg);
        Logger.Success($"Posted comment from {token.UserId}: \"{msg}\" on user {id}", LogArea.Comments);

        return this.Redirect("~/slot/" + id);
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        Slot? heartedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (heartedSlot == null) return this.NotFound();

        await this.database.HeartLevel(token.UserId, heartedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        Slot? heartedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (heartedSlot == null) return this.NotFound();

        await this.database.UnheartLevel(token.UserId, heartedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> QueueLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        Slot? queuedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (queuedSlot == null) return this.NotFound();

        await this.database.QueueLevel(token.UserId, queuedSlot);

        return this.Redirect(callbackUrl);
    }

    [HttpGet("unqueue")]
    public async Task<IActionResult> UnqueueLevel([FromRoute] int id, [FromQuery] string? callbackUrl)
    {
        if (string.IsNullOrEmpty(callbackUrl)) callbackUrl = "~/slot/" + id;

        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        Slot? queuedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (queuedSlot == null) return this.NotFound();

        await this.database.UnqueueLevel(token.UserId, queuedSlot);

        return this.Redirect(callbackUrl);
    }
}