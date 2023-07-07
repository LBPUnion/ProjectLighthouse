using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[Route("{type}/{id:int}")]
public class CommentController : ControllerBase
{
    private readonly DatabaseContext database;

    public CommentController(DatabaseContext database)
    {
        this.database = database;
    }

    private static CommentType? ParseType(string type) =>
        type switch
        {
            "slot" => CommentType.Level,
            "user" => CommentType.Profile,
            _ => null,
        };

    [HttpGet("rateComment")]
    public async Task<IActionResult> RateComment(string type, int id, [FromQuery] int? commentId, [FromQuery] int? rating, [FromQuery] string? redirect)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        CommentType? commentType = ParseType(type);
        if (commentType == null) return this.BadRequest();

        await this.database.RateComment(token.UserId, commentId.GetValueOrDefault(), rating.GetValueOrDefault());

        return this.Redirect(redirect ?? $"~/user/{id}#{commentId}");
    }

    [HttpPost("postComment")]
    public async Task<IActionResult> PostComment(string type, int id, [FromForm] string? msg, [FromQuery] string? redirect)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        CommentType? commentType = ParseType(type);
        if (commentType == null) return this.BadRequest();

        if (msg == null)
        {
            Logger.Error($"Refusing to post comment from {token.UserId} on {commentType} {id}, {nameof(msg)} is null",
                LogArea.Comments);
            return this.Redirect("~/user/" + id);
        }

        string username = await this.database.UsernameFromWebToken(token);
        string filteredText = CensorHelper.FilterMessage(msg);

        if (ServerConfiguration.Instance.LogChatFiltering && filteredText != msg)
            Logger.Info(
                $"Censored profane word(s) from {commentType} comment sent by {username}: \"{msg}\" => \"{filteredText}\"",
                LogArea.Filter);

        bool success = await this.database.PostComment(token.UserId, id, (CommentType)commentType, filteredText);
        if (success)
        {
            Logger.Success($"Posted comment from {username}: \"{filteredText}\" on {commentType} {id}",
                LogArea.Comments);
        }
        else
        {
            Logger.Error($"Failed to post comment from {username}: \"{filteredText}\" on {commentType} {id}",
                LogArea.Comments);
        }

        return this.Redirect(redirect ?? $"~/{type}/" + id);
    }
}