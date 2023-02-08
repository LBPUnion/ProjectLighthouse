#nullable enable
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
[Route("user/{id:int}")]
public class UserPageController : ControllerBase
{
    private readonly Database database;

    public UserPageController(Database database)
    {
        this.database = database;
    }

    [HttpGet("rateComment")]
    public async Task<IActionResult> RateComment([FromRoute] int id, [FromQuery] int? commentId, [FromQuery] int? rating)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        await this.database.RateComment(token.UserId, commentId.GetValueOrDefault(), rating.GetValueOrDefault());

        return this.Redirect($"~/user/{id}#{commentId}");
    }

    [HttpPost("postComment")]
    public async Task<IActionResult> PostComment([FromRoute] int id, [FromForm] string? msg)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        if (msg == null)
        {
            Logger.Error($"Refusing to post comment from {token.UserId} on user {id}, {nameof(msg)} is null", LogArea.Comments);
            return this.Redirect("~/user/" + id);
        }

        // Prevent potential xml injection and censor content
        msg = SanitizationHelper.SanitizeString(msg);
        msg = CensorHelper.FilterMessage(msg);

        bool success = await this.database.PostComment(token.UserId, id, CommentType.Profile, msg);
        if (success)
        {
            Logger.Success($"Posted comment from {token.UserId}: \"{msg}\" on user {id}", LogArea.Comments);
        }
        else
        {
            Logger.Error($"Failed to post comment from {token.UserId}: \"{msg}\" on user {id}", LogArea.Comments);
        }

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartUser([FromRoute] int id)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(token.UserId, heartedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartUser([FromRoute] int id)
    {
        WebToken? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(token.UserId, heartedUser);

        return this.Redirect("~/user/" + id);
    }
}