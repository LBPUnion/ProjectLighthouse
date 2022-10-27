#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

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

    [HttpGet("userComment/{username}")]
    [HttpGet("comment/user/{slotId}")]
    public async Task<IActionResult> GetSingleComment([FromQuery] int commentId, string? username, int? slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403);
        if (username == null && slotId == null) return this.BadRequest();
        User? userTarget = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        Slot? slotTarget = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (userTarget == null && slotTarget == null) return this.StatusCode(404);

        Comment? comment = await this.database.Comments.Include(c => c.Poster)
                                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.StatusCode(404);

        return this.Ok(comment.Serialize());
    }

    [HttpPost("rateUserComment/{username}")]
    [HttpPost("rateComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> RateComment([FromQuery] int commentId, [FromQuery] int rating, string? username, string? slotType, int slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (username == null && (SlotHelper.IsTypeInvalid(slotType) || slotId == 0)) return this.BadRequest();

        bool success = await this.database.RateComment(token.UserId, commentId, rating);
        if (!success) return this.BadRequest();

        return this.Ok();
    }

    [HttpGet("comments/{slotType}/{slotId:int}")]
    [HttpGet("userComments/{username}")]
    public async Task<IActionResult> GetComments([FromQuery] int pageStart, [FromQuery] int pageSize, string? username, string? slotType, int slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        int targetId = slotId;
        CommentType type = CommentType.Level;
        if (!string.IsNullOrWhiteSpace(username))
        {
            targetId = this.database.Users.First(u => u.Username.Equals(username)).UserId;
            type = CommentType.Profile;
        }
        else
        {
            if (SlotHelper.IsTypeInvalid(slotType) || slotId == 0) return this.BadRequest();
        }

        if (type == CommentType.Level && slotType == "developer") targetId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        if (type == CommentType.Profile)
        {
            User? profile = await this.database.Users.FirstOrDefaultAsync(s => s.UserId == targetId);
            if (profile == null) return this.BadRequest();
            if (!profile.CommentsEnabled) return this.NotFound();
        }
        else
        {
            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == targetId);
            if (slot == null) return this.BadRequest();
            if (!slot.CommentsEnabled) return this.NotFound();
        }

        List<Comment> comments = await this.database.Comments.Include
                (c => c.Poster)
            .Where(c => c.TargetId == targetId && c.Type == type)
            .OrderByDescending(c => c.Timestamp)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();

        string outputXml = comments.Aggregate
            (string.Empty, (current, comment) => current + comment.Serialize(this.getReaction(token.UserId, comment.CommentId).Result));
        return this.Ok(LbpSerializer.StringElement("comments", outputXml));
    }

    private async Task<int> getReaction(int userId, int commentId)
    {
        return await this.database.Reactions.Where(r => r.UserId == userId && r.TargetId == commentId)
            .Select(r => r.Rating)
            .FirstOrDefaultAsync();
    }

    [HttpPost("postUserComment/{username}")]
    [HttpPost("postComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> PostComment(string? username, string? slotType, int slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Comment));
        Comment? comment = (Comment?)serializer.Deserialize(new StringReader(bodyString));

        SanitizationHelper.SanitizeStringsInClass(comment);

        CommentType type = (slotId == 0 ? CommentType.Profile : CommentType.Level);

        if (type == CommentType.Level && (SlotHelper.IsTypeInvalid(slotType) || slotId == 0)) return this.BadRequest();

        if (comment == null) return this.BadRequest();

        int targetId = slotId;

        if (type == CommentType.Profile) targetId = this.database.Users.First(u => u.Username == username).UserId;

        if (slotType == "developer") targetId = await SlotHelper.GetPlaceholderSlotId(this.database, targetId, SlotType.Developer);

        bool success = await this.database.PostComment(token.UserId, targetId, type, comment.Message);
        if (success) return this.Ok();

        return this.BadRequest();
    }

    [HttpPost("deleteUserComment/{username}")]
    [HttpPost("deleteComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> DeleteComment([FromQuery] int commentId, string? username, string? slotType, int slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Comment? comment = await this.database.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.NotFound();

        if (comment.Type == CommentType.Level && (SlotHelper.IsTypeInvalid(slotType) || slotId == 0)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        // if you are not the poster
        if (comment.PosterUserId != token.UserId)
        {
            if (comment.Type == CommentType.Profile)
            {
                // if you aren't the poster and aren't the profile owner
                if (comment.TargetId != token.UserId)
                {
                    return this.StatusCode(403, "");
                }
            }
            else
            {
                Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == comment.TargetId);
                // if you aren't the creator of the level
                if (slot == null || slot.CreatorId != token.UserId || slotId != slot.SlotId)
                {
                    return this.StatusCode(403, "");
                }
            }
        }

        comment.Deleted = true;
        comment.DeletedBy = await this.database.UsernameFromGameToken(token);
        comment.DeletedType = "user";

        await this.database.SaveChangesAsync();

        return this.Ok();
    }
}