#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CommentController : ControllerBase
{
    private readonly Database database;
    public CommentController(Database database)
    {
        this.database = database;
    }

    [HttpPost("rateUserComment/{username}")]
    [HttpPost("rateComment/user/{slotId:int}")]
    public async Task<IActionResult> RateComment([FromQuery] int commentId, [FromQuery] int rating, string? username, int? slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        bool success = await this.database.RateComment(user, commentId, rating);
        if (!success) return this.BadRequest();

        return this.Ok();
    }

    [HttpGet("comments/user/{slotId:int}")]
    [HttpGet("userComments/{username}")]
    public async Task<IActionResult> GetComments([FromQuery] int pageStart, [FromQuery] int pageSize, string? username, int? slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        int targetId = slotId.GetValueOrDefault();
        CommentType type = CommentType.Level;
        if (!string.IsNullOrWhiteSpace(username))
        {
            targetId = this.database.Users.First(u => u.Username.Equals(username)).UserId;
            type = CommentType.Profile;
        }

        List<Comment> comments = await this.database.Comments.Include
                (c => c.Poster)
            .Where(c => c.TargetId == targetId && c.Type == type)
            .OrderByDescending(c => c.Timestamp)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();

        string outputXml = comments.Aggregate
            (string.Empty, (current, comment) => current + comment.Serialize(this.getReaction(user.UserId, comment.CommentId).Result));
        return this.Ok(LbpSerializer.StringElement("comments", outputXml));
    }

    private async Task<int> getReaction(int userId, int commentId)
    {
        Reaction? reaction = await this.database.Reactions.FirstOrDefaultAsync(r => r.UserId == userId && r.TargetId == commentId);
        if (reaction == null) return 0;

        return reaction.Rating;
    }

    [HttpPost("postUserComment/{username}")]
    [HttpPost("postComment/user/{slotId:int}")]
    public async Task<IActionResult> PostComment(string? username, int? slotId)
    {
        User? poster = await this.database.UserFromGameRequest(this.Request);
        if (poster == null) return this.StatusCode(403, "");

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Comment));
        Comment? comment = (Comment?)serializer.Deserialize(new StringReader(bodyString));

        SanitizationHelper.SanitizeStringsInClass(comment);

        CommentType type = (slotId.GetValueOrDefault() == 0 ? CommentType.Profile : CommentType.Level);

        if (comment == null) return this.BadRequest();

        int targetId = slotId.GetValueOrDefault();

        if (type == CommentType.Profile) targetId = this.database.Users.First(u => u.Username == username).UserId;

        bool success = await this.database.PostComment(poster, targetId, type, comment.Message);
        if (success) return this.Ok();

        return this.BadRequest();
    }

    [HttpPost("deleteUserComment/{username}")]
    [HttpPost("deleteComment/user/{slotId:int}")]
    public async Task<IActionResult> DeleteComment([FromQuery] int commentId, string? username, int? slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Comment? comment = await this.database.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.NotFound();

        // if you are not the poster
        if (comment.PosterUserId != user.UserId)
        {
            if (comment.Type == CommentType.Profile)
            {
                // if you aren't the poster and aren't the profile owner
                if (comment.TargetId != user.UserId)
                {
                    return this.StatusCode(403, "");
                }
            }
            else
            {
                Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == comment.TargetId);
                // if you aren't the creator of the level
                if (slot == null || slot.CreatorId != user.UserId || slotId.GetValueOrDefault() != slot.SlotId)
                {
                    return this.StatusCode(403, "");
                }
            }
        }

        comment.Deleted = true;
        comment.DeletedBy = user.Username;
        comment.DeletedType = "user";

        await this.database.SaveChangesAsync();

        return this.Ok();
    }
}