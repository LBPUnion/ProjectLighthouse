#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
[Route("user/{id:int}")]
public class UserPageController : ControllerBase
{
    private readonly DatabaseContext database;

    public UserPageController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("rateComment")]
    public async Task<IActionResult> RateComment([FromRoute] int id, [FromQuery] int? commentId, [FromQuery] int? rating)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        await this.database.RateComment(token.UserId, commentId.GetValueOrDefault(), rating.GetValueOrDefault());

        return this.Redirect($"~/user/{id}#{commentId}");
    }

    [HttpPost("postComment")]
    public async Task<IActionResult> PostComment([FromRoute] int id, [FromForm] string? msg)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        // Deny request if in read-only mode
        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return this.Redirect("~/user/" + id);

        if (msg == null)
        {
            Logger.Error($"Refusing to post comment from {token.UserId} on user {id}, {nameof(msg)} is null", LogArea.Comments);
            return this.Redirect("~/user/" + id);
        }

        string username = await this.database.UsernameFromWebToken(token);
        string filteredText = CensorHelper.FilterMessage(msg, FilterLocation.UserComment, username);

        bool success = await this.database.PostComment(token.UserId, id, CommentType.Profile, filteredText);
        if (success)
        {
            Logger.Success($"Posted comment from {username}: \"{filteredText}\" on user {id}", LogArea.Comments);
        }
        else
        {
            Logger.Error($"Failed to post comment from {username}: \"{filteredText}\" on user {id}", LogArea.Comments);
        }

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(token.UserId, heartedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(token.UserId, heartedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("block")]
    public async Task<IActionResult> BlockUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? blockedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (blockedUser == null) return this.NotFound();

        await this.database.BlockUser(token.UserId, blockedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("unblock")]
    public async Task<IActionResult> UnblockUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? blockedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (blockedUser == null) return this.NotFound();

        await this.database.UnblockUser(token.UserId, blockedUser);

        return this.Redirect("~/user/" + id);
    }
}