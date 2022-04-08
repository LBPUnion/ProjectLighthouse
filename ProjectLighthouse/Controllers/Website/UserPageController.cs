#nullable enable
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Website;

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
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        await this.database.RateComment(user, commentId.GetValueOrDefault(), rating.GetValueOrDefault());

        return this.Redirect($"~/user/{id}#{commentId}");
    }

    [HttpPost("postComment")]
    public async Task<IActionResult> PostComment([FromRoute] int id, [FromForm] string? msg)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (msg == null)
        {
            Logger.Log($"Refusing to post comment from {user.UserId} on user {id}, {nameof(msg)} is null", LoggerLevelComments.Instance);
            return this.Redirect("~/user/" + id);
        }

        msg = SanitizationHelper.SanitizeString(msg);

        await this.database.PostComment(user, id, CommentType.Profile, msg);
        Logger.Log($"Posted comment from {user.UserId}: \"{msg}\" on user {id}", LoggerLevelComments.Instance);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartUser([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(user, heartedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartUser([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        User? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(user, heartedUser);

        return this.Redirect("~/user/" + id);
    }
}